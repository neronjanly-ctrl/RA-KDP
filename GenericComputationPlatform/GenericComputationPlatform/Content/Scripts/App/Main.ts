/// <reference path="../../../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />

function setCreateMessage(msg: string, error: boolean) {
    if (!error) {
        $("#formMessage").html(msg).removeClass("text-danger").addClass("text-info");
    }
    else {
        $("#formMessage").html(msg).removeClass("text-info").addClass("text-danger");
    }
}

// ReSharper disable once InconsistentNaming
declare let NGL: any;

function setupProteinInfoModal() {
    $("#proteinInfo").on("show.bs.modal",
        function (event) {
            const a = $(event.relatedTarget as any);
            const pname = a.data("pname");

            const modal = $(this);
            const href = a.attr("href");
            if (href === undefined)
                return;

            modal.find(".maximizeBtn").attr("href", href);
            modal.find(".modal-title").text(pname);
            modal.find(".modal-body").text("Loading...");

            modal.find(".modal-body").load(href,
                () => {
                    modal.find(".modal-title").html($("#proteinHeader").html());

                    setTimeout(() => {
                        const stage = new NGL.Stage("viewport", { backgroundColor: "white" });
                        const url = $("link[rel=model]").attr("href");

                        stage.loadFile(url).then(function (o: any) {
                            o.addRepresentation("cartoon", {
                                colorScheme: "residueindex",
                                colorScale: "spectral",
                                colorReverse: true,
                                quality: "high"
                            });
                            o.autoView();
                        });
                    }, 500);
                });
        });
}

function setupFingerprintInfoModal() {
    $("#fingerprintInfo").on("show.bs.modal",
        function (event) {
            const a = $(event.relatedTarget as any);
            const fptype = a.data("fptype");

            const modal = $(this);
            const href = a.attr("href");
            if (href === undefined)
                return;

            modal.find(".maximizeBtn").attr("href", href);
            modal.find(".modal-title").text(fptype);

            if (a.data("fp"))
                modal.find(".modal-body").text(a.data("fp"));
            else {
                modal.find(".modal-body").text("Loading...");
                modal.find(".modal-body").load(href, o => a.data("fp", o));
            }
        });
}

function setupCompareLigandsModal() {
    $("#compareLigands").on("show.bs.modal",
        function (event) {
            //console.log(`compareLigands.show.bs.modal`);

            const a = $(event.relatedTarget as any);
            const pname = a.data("pname");

            const modal = $(this);
            const href = a.attr("href");
            if (href === undefined)
                return;

            modal.find(".maximizeBtn").attr("href", href);
            modal.find(".modal-title").text(pname);
            modal.find(".modal-body").text("Loading...");

            modal.find(".modal-body").load(href, () => {
                modal.find(".modal-title").html($("#ligandHeader").html());
                let fp0 = modal.find(".fp").eq(0), fp1 = modal.find(".fp").eq(1);
                let t0 = fp0.text(), t1 = fp1.text();
                let nt0 = "", nt1 = "";
                if (t0.length === t1.length) {
                    let inb = false;

                    for (let i = 0; i < t0.length; i++) {
                        if (t0[i] == t1[i]) {
                            if (inb) {
                                nt0 += "</b>";
                                nt1 += "</b>";
                                inb = false;
                            }
                        }
                        else {
                            if (!inb) {
                                nt0 += "<b>";
                                nt1 += "<b>";
                                inb = true;
                            }
                        }
                        nt0 += t0[i];
                        nt1 += t1[i];
                    }

                    if (inb) {
                        nt0 += "</b>";
                        nt1 += "</b>";
                    }

                    fp0.html(nt0);
                    fp1.html(nt1);
                }
            });
        });
}

// create job page UI logic
function toggleMolRow(input: HTMLElement) {
    const index = $(input).parents(".mol-name-row").prevAll(".mol-name-row").length;
    setActiveMol(index);
}

function addRemoveBtn(input: JQuery<HTMLElement>) {
    input.css("width", "calc(100% - 110px)")
        .after($("<button class=\"btn btn-danger float-right\" style=\"width:100px\">Remove</button>"))
        .next()
        .click(ev => {
            removeMol($(ev.target));
        });
}

function removeRemoveBtn(input: JQuery<HTMLElement>) {
    input.css("width", "")
        .next()
        .remove();
}

declare let JSApplet: any;
let maxLigands: number = 3;
let apps: JsmeApplet[] = [];

function setActiveMol(index: number) {
    const rows = $(".mol-row");
    if (rows.length === 0)
        return;

    const safeIndex = Math.max(0, Math.min(index, rows.length - 1));
    rows.hide().eq(safeIndex).show();

    const tabs = $(".ra-ligand-tab");
    tabs.removeClass("active").attr("aria-selected", "false");
    tabs.eq(safeIndex).addClass("active").attr("aria-selected", "true");
}

function getLigandTabTitle(index: number): string {
    const nameValue = $(`input[name='ligandName${index}']`).val();
    if (typeof nameValue === "string" && nameValue.trim() !== "")
        return nameValue.trim();

    return `Ligand ${index + 1}`;
}

function refreshLigandTabs() {
    const tabBar = $("#ligandTabs");
    if (tabBar.length === 0)
        return;

    const rows = $(".mol-row");
    const count = rows.length;
    tabBar.empty();

    if (count <= 1) {
        tabBar.hide();
        return;
    }

    const visibleIndex = Math.max(0, rows.index($(".mol-row:visible")));
    tabBar.show();

    for (let i = 0; i < count; i++) {
        const smiles = apps[i] ? apps[i].smiles() : "";
        const stateClass = smiles === "" ? "needs-input" : "ready";
        const stateText = smiles === "" ? "Input needed" : "Ready";
        const tab = $(`<button type="button" class="ra-ligand-tab ${stateClass}" role="tab" aria-selected="false"></button>`);

        tab.append($("<span class=\"ra-ligand-tab-index\"></span>").text(`${i + 1}`));
        tab.append($("<span class=\"ra-ligand-tab-name\"></span>").text(getLigandTabTitle(i)));
        tab.append($("<span class=\"ra-ligand-tab-state\"></span>").text(stateText));
        tab.click(() => setActiveMol(i));
        tabBar.append(tab);
    }

    setActiveMol(visibleIndex);
}

function setupSmilesDirectInput(row: JQuery<HTMLElement>) {
    const input = row.find(".smiles-direct-input");
    const button = row.find(".smiles-apply");

    button.click(ev => {
        ev.preventDefault();
        applySmilesDirectInput($(ev.currentTarget));
    });

    input.keypress(event => {
        if (event.which !== 13)
            return true;

        event.preventDefault();
        applySmilesDirectInput(button);
        return false;
    });
}

function applySmilesDirectInput(button: JQuery<HTMLElement>) {
    const row = button.parents(".mol-row");
    const index = row.prevAll(".mol-row").length;
    const input = row.find(".smiles-direct-input");
    const message = row.find(".smiles-import-msg");
    const value = input.val();
    const smiles = typeof value === "string" ? value.trim() : "";

    if (smiles === "") {
        message.text("Paste a SMILES string first.").removeClass("text-info").addClass("text-danger");
        return;
    }

    try {
        apps[index].readGenericMolecularInput(smiles);
        apps[index].repaint();

        if (apps[index].smiles() === "") {
            message.text("The editor could not read this molecular input. Please check the SMILES string.").removeClass("text-info").addClass("text-danger");
            return;
        }

        $(`#mol${index}ErrMsg`).hide().removeClass("show");
        message.text("SMILES loaded into the molecular editor.").removeClass("text-danger").addClass("text-info");
        refreshLigandTabs();
    }
    catch {
        message.text("The editor could not read this molecular input. Please check the SMILES string.").removeClass("text-info").addClass("text-danger");
    }
}

function removeMol(button: JQuery<HTMLElement>) {
    let count = $(".mol-row").length;

    let molNameLabel = "Molecule Name";
    let molLabel = "Molecule";
    let molNameId = "ligandName";
    let molId = "mol";

    let row1 = button.parents(".mol-name-row");
    let row2 = row1.next();
    let prevCount = row1.prevAll(".mol-name-row").length;

    let visible = row2.is(":visible");
    let nextActive = visible ? Math.min(count - 2, prevCount) : $(".mol-row").index($(".mol-row:visible"));

    apps.splice(prevCount, 1);

    row1.nextAll(".mol-name-row").each((_i, el) => {
        let row1 = $(el);
        let row2 = row1.next();
        row1.find("strong").text(`${molNameLabel} ${prevCount + 1}`);
        row1.find("label").attr("for", `${molNameId}${prevCount}`);
        row1.find("input").attr("id", `${molNameId}${prevCount}`).attr("name", `${molNameId}${prevCount}`)
        row1.find(".alert").attr("id", `${molNameId}${prevCount}ErrMsg`);
        row2.find("strong").text(`${molLabel} ${prevCount + 1}`);
        row2.children("label").attr("for", `${molId}${prevCount}`);
        row2.find(".mol-editor").attr("id", `${molId}${prevCount}`);
        row2.find(".alert").attr("id", `${molId}${prevCount}ErrMsg`);
        row2.find(".smiles-direct-label").attr("for", `${molId}${prevCount}SmilesInput`);
        row2.find(".smiles-direct-input").attr("id", `${molId}${prevCount}SmilesInput`);
        prevCount++;
    });

    row1.remove();
    row2.remove();

    // deleted the last but one molecule
    if (count === 2) {
        $(".mol-name-row strong").text(molNameLabel);
        $(".mol-row strong").text(molLabel);
        removeRemoveBtn($(".mol-name-row input"));
    }

    const remaining = $(".mol-row").length;
    if (remaining > 0) {
        if (nextActive < 0)
            nextActive = 0;

        setActiveMol(Math.min(remaining - 1, nextActive));
    }

    $("button#addmol").show();
    setCreateMessage("", false);
    refreshLigandTabs();
}

function addMol() {
    let count = $(".mol-row").length;

    if (count >= maxLigands) {
        setCreateMessage(`You can add up to ${maxLigands} molecules.`, true);
        return;
    }

    let molNameLabel = "Molecule Name";
    let molLabel = "Molecule";
    let molNameId = `ligandName${count}`;
    let molId = `mol${count}`;

    if (count === 1) {
        $(".mol-name-row strong").text(`${molNameLabel} 1`);
        $(".mol-row strong").text(`${molLabel} 1`);
    }

    if (count > 0) {
        molNameLabel = `${molNameLabel} ${count + 1}`;
        molLabel = `${molLabel} ${count + 1}`;
    }

    let row1 = $(`
<div class="form-group row mol-name-row">
    <label class="col-sm-2 col-form-label" for="${molNameId}">
        <span class="ra-step-badge mr-2">2</span>
        <strong>${molNameLabel}</strong>
    </label>
    <div class="col-sm-10">
        <input type="text" class="form-control d-inline-block" id="${molNameId}" name="${molNameId}" placeholder="e.g. Aspirin"/>
        <div class="alert alert-danger fade collapse mt-2" role="alert" id="${molNameId}ErrMsg"/>
        <small class="form-text text-muted">Provide the name of the molecule to be computed.</small>
    </div>
</div>
`);

    let row2 = $(`
<div class="form-group row mol-row">
    <label class="col-sm-2" for="${molId}">
        <span class="ra-step-badge mr-2">2</span>
        <strong>${molLabel}</strong>
    </label>
    <div class="col-sm-10">
        <div id="${molId}" class="mol-editor" width="100%" height="380px"/>
        <div class="alert alert-danger fade collapse mt-2" role="alert" id="${molId}ErrMsg"/>
        <div class="ra-ligand-helper">
            <label class="smiles-direct-label" for="${molId}SmilesInput">Paste SMILES directly</label>
            <div class="input-group input-group-sm ra-smiles-input-group">
                <input type="text" class="form-control smiles-direct-input" id="${molId}SmilesInput" placeholder="e.g. CC(=O)OC1=CC=CC=C1C(=O)O" autocomplete="off"/>
                <div class="input-group-append">
                    <button type="button" class="btn btn-outline-primary smiles-apply">Apply to editor</button>
                </div>
            </div>
            <small class="form-text text-muted">You can also draw manually, click the blue double-triangle button on the right of the toolbar, or right-click the blank editor area to paste MOL, SDF, or SMILES.</small>
            <div class="small mt-1 smiles-import-msg" aria-live="polite"></div>
        </div>
    </div>
</div>
`);

    // insert rows
    const ligandRows = $("#ligandRows");
    if (count === 0) {
        if (ligandRows.length > 0)
            ligandRows.append(row1).append(row2);
        else
            $(".job-name-row").after(row2).after(row1);
    }
    else {
        if (ligandRows.length > 0)
            ligandRows.append(row1).append(row2);
        else
            $(".mol-row:last").after(row2).after(row1);

        // add Remove button
        if (count === 1) {
            addRemoveBtn($(".mol-name-row:first input"));
        }
        addRemoveBtn($(".mol-name-row:last input"));
    }
    count++;

    apps.push(new JSApplet.JSME(molId, "100%", "380px", {
        "options": "nozoom,noshowdraganddropsymbolindepictmode",
        "jme": "",
    }));

    // add event handler for toggling display
    $(".mol-name-row:last input")
        .focus(ev => toggleMolRow(ev.target))
        .keyup(() => refreshLigandTabs());

    setupSmilesDirectInput(row2);
    setActiveMol(count - 1);
    refreshLigandTabs();

    if (count === maxLigands)
        $("button#addmol").hide();
}

function hookJobCreateButtons() {
    let submitting = false;

    $("button#create").click(ev => {
        ev.preventDefault();
        if (submitting)
            return;
        submitting = true;

        const url = $(ev.target).prev("a").attr("href") as string;
        const jobName = $("input[name='jobName']").val();
        const isPrivate = $("input[name='isPrivate']").prop("checked");

        let done: boolean = true;
        if (jobName === "") {
            $("#jobNameRequired").addClass("show").show();
            done = false;
        } else {
            $("#jobNameRequired").hide().removeClass("show");
        }

        let ligandCount = $(".mol-row").length;

        let formData: { [key: string]: any } = {
            "JobName": jobName,
            "IsPrivate": isPrivate === true
        };

        let firstBad = -1;
        let nameSet = new Set<string>();
        let smilesSet = new Set<string>();

        for (let i = 0; i < ligandCount; i++) {
            let ligandName = $(`input[name='ligandName${i}']`).val();
            const smiles = apps[i].smiles();

            let nameLabel = ligandCount > 1 ? `Molecule Name ${i + 1}` : "Molecule Name";
            if (typeof (ligandName) !== "string" || (ligandName = ligandName.trim()) === "") {
                $(`#ligandName${i}ErrMsg`).html(`<strong>${nameLabel}</strong> is required.`).addClass("show").show();
                done = false;
                if (firstBad === -1)
                    firstBad = i;
            } else if (nameSet.has(ligandName)) {
                $(`#ligandName${i}ErrMsg`).html(`<strong>${nameLabel}</strong> is duplicate.`).addClass("show").show();
                done = false;
                if (firstBad === -1)
                    firstBad = i;
            } else {
                nameSet.add(ligandName);
                $(`#ligandName${i}ErrMsg`).hide().removeClass("show");
            }

            let molLabel = ligandCount > 1 ? `Molecule ${i + 1}` : "Molecule";
            if (smiles === "") {
                $(`#mol${i}ErrMsg`).html(`<strong>${molLabel}</strong> is required.`).addClass("show").show();
                done = false;
                if (firstBad === -1)
                    firstBad = i;
            } else if (smilesSet.has(smiles)) {
                $(`#mol${i}ErrMsg`).html(`<strong>${molLabel}</strong> is duplicate.`).addClass("show").show();
                done = false;
                if (firstBad === -1)
                    firstBad = i;
            } else {
                smilesSet.add(smiles);
                $(`#mol${i}ErrMsg`).hide().removeClass("show");
            }

            if (done) {
                formData[`Ligands[${i}].name`] = ligandName;
                formData[`Ligands[${i}].smiles`] = smiles;
            }
        }

        if (firstBad !== -1 && !$(".mol-row").eq(firstBad).is(":visible")) {
            $(".mol-row:visible").hide();
            $(".mol-row").eq(firstBad).show();
        }

        if (!done) {
            refreshLigandTabs();
            submitting = false;
            return;
        }

        const targetMode = $("input[name='targetMode']:checked").val() as string || "all";
        formData["TargetMode"] = targetMode;

        if (targetMode === "selected") {
            const selectedTargets = $(".target-checkbox:checked").map((_, el) => $(el).val() as string).get();

            if (selectedTargets.length === 0) {
                $("#targetRequired").addClass("show").show();
                submitting = false;
                return;
            }

            $("#targetRequired").hide().removeClass("show");

            selectedTargets.forEach((value, index) => {
                formData[`SelectedProteinIds[${index}]`] = value;
            });
        } else {
            $("#targetRequired").hide().removeClass("show");
        }

        setCreateMessage(`Creating new job "${jobName}"...`, false);

        formData["MZ-CSRF"] = $("input[name='MZ-CSRF']").val();

        $.post($("form").prop("action"),
            formData,
            data => {
                if (data) {
                    setCreateMessage("Created successfully.", false);
                    window.location.href = url.replace(/0$/, data);
                } else {
                    setCreateMessage(`Failed to create new job.`, true);
                }
                submitting = false;
            }).fail(data => {
                setCreateMessage(`Failed: ${data.statusText}`, true);
                submitting = false;
            });
    });

    $(".mol-name-row input").focus(ev => toggleMolRow(ev.target));

    $("button#addmol").click(ev => {
        ev.preventDefault();

        if (submitting)
            return;
        addMol();
    });
}

function jsmeOnLoad() {
    addMol();
}

function setMaxLigands(value: number) {
    maxLigands = value;
}

function loadPreContents() {
    $("table pre").each(function () {
        const pre = $(this as any);
        const href = pre.prev("link").attr("href");
        if (!href) return;

        $.get(href,
            data => {
                pre.html(data);
            });
    });
}

function updateBbbBar(b: JQuery<HTMLElement>, score: number) {
    b.text(score.toFixed(2));
    const g = b.parent().nextAll("svg").children(".scoreBar");
    const scale = g.data("scale") as number, offset = g.data("offset") as number;
    const line = g.children("line");
    const text = g.children("text");
    const x = offset + scale * score, dx = parseFloat(text.attr("x") as string);
    line.attr("x1", x).attr("x2", x);
    text.text(`Score ${score.toFixed(2)}`).attr("x", x + dx);
    g.removeClass("d-none");
}

function loadBbbScores() {
    $(".bbbscore").each(function () {
        let score = parseFloat($(this).text());
        if (score) {
            updateBbbBar($(this), score as number);
        }
    });

    $("link[rel=bbbscore]").each(function () {
        const link = $(this as any);
        const href = link.attr("href");
        if (!href) return;

        $.get(href,
            data => {
                updateBbbBar(link.prev("b"), parseFloat(data));
            });
    });
}

interface ChemblCompound {
    id: string;
    smiles: string;
    activity: string;
    fingerprint: Blob;
    similarity: number;
};

interface Prediction {
    activity: string;
    confidenceLevel: number;
};

function setupJobHubConnection() {
    const hubUrl = $("a#jobhub").attr("href");
    if (hubUrl === undefined)
        return;

    const connection = new signalR
        .HubConnectionBuilder()
        .withUrl(hubUrl)
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("SetProgress",
        (progress: number | string,
            timeRemaining: string,
            utms: number) => {
            //console.log([new Date().toUTCString(), "SetProgress", progress, timeRemaining, utms]);
            JobProgressHelper.setProgress(progress, timeRemaining, utms);
        });

    connection.on("SetResult",
        (cavityId: string,
            ligandId: string,
            fullProteinSymbol: string,
            dockingScores: (number | null)[],
            dockingIds: (string | null)[],
            mostSimilarActiveCompound: ChemblCompound | null,
            mostSimilarCompound: ChemblCompound | null,
            prediction: Prediction | null) => {
            cavityId = cavityId.substring(0, 8);
            ligandId = ligandId.substring(0, 8);
            //console.log([new Date().toUTCString(), "SetResult", cavityId, ligandId, fullProteinSymbol, dockingScores, dockingIds, mostSimilarActiveCompound, mostSimilarCompound, prediction]);
            JobResultHelper.setResult(cavityId, ligandId, dockingScores, dockingIds, mostSimilarActiveCompound, prediction);
        });

    connection.on("SetStatus",
        (status: string,
            utms: number) => {
            //console.log([new Date().toUTCString(), "SetStatus", status, utms]);
            JobStatusHelper.setStatus(status, utms);
            if (status === "Finished" || status === "Failed")
                connection.stop();
        });

    connection.start().catch((reason: any) => {
        return console.error(reason.toString());
    });
}

function setupViewport(model: InputData) {
    const jvp = $("canvas.viewport");
    const jtb = $("aside.toolbox");

    let imgUrls: { [key: string]: string } = {};
    $("a.depiction").each(function () {
        let href = $(this).attr("href");
        if (href !== undefined) {
            let lig = $(this).attr("id");
            if (!lig?.startsWith("lig-"))
                console.error(`a.depiction[href=${href}] has no lig- id`);
            else {
                lig = lig.substring("lig-".length);
                imgUrls[lig] = href;
            }
        }
        else
            console.error("no href found for a.depiction");
    });

    if (jvp.length !== 0 && jtb.length !== 0) {
        if (Object.keys(imgUrls).length !== model.drugs.length)
            console.error(`the number of depictions(${Object.keys(imgUrls).length}) must equal to that of drugs(${model.drugs.length})`);

        const vp = new ViewportControl(jvp, model, imgUrls);
        const tb = new ToolboxControl(jtb);

        vp.select(style => {
            tb.setStyle(style);
            tb.show();
        }).cancel(() => {
            tb.hide();
        }).multiselect(() => {
            tb.hide();
        });

        tb.change(style => {
            vp.setSelectionStyle(style);
            vp.invalidate();
        });
    }
}

$("#cookieConsent button[data-cookie-string]").click(el => {
    if (el.target.dataset.cookieString !== undefined)
        document.cookie = el.target.dataset.cookieString;
});

$(".openee").click(el => {
    let tag = $(el.target).data("tag");
    let email = window.atob(tag);
    window.open(`mailto:${email}`);
    return false;
});

function utmsToString(utms: number): string {
    return new Date(utms).toLocaleString();
}

$(".update-utms").each((i, el) => {
    const utms = $(el).data("utms");
    if (utms !== undefined)
        $(el).text(utmsToString(utms));
});

$(".use-gene-symbol").click(el => {
    document.cookie = "UseGeneSymbolForTargetDisplay=true; path=/";
    document.location.reload();
    return false;
});

$(".use-protein-symbol").click(el => {
    document.cookie = "UseGeneSymbolForTargetDisplay=false; path=/";
    document.location.reload();
    return false;
});
