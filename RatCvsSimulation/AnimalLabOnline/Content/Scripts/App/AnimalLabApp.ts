declare let JSApplet: any;

class AnimalLabApp {
    static current: AnimalLabApp;

    animationManager?: AnimationManager;
    jsmeApplet?: JsmeApplet;

    private setupBarChartModalForResults(modal: JQuery<HTMLElement>) {
        let svgChart = d3.select("#chart-modal svg.chart");
        let chart = new ChartRenderer(Size.fromViewBox(svgChart.attr("viewBox")), svgChart.node() as SVGGElement);
        modal.on("show.bs.modal", event => {
            const button = $(event.relatedTarget as HTMLElement);
            $("#chart-modal-title").text(button.text());

            let indicator = button.data("indicator") as string;
            let unit = button.data("unit") as string;
            let results = button.data("results") as [number, number][];
            chart.draw(indicator, unit, results);
        });
    }

    private setupGroupDefModalForResults(modal: JQuery<HTMLElement>) {
        modal.on("show.bs.modal", event => {
            const button = $(event.relatedTarget as HTMLElement);
            const rows = modal.find("table tr") as JQuery<Node> as JQuery<HTMLTableRowElement>;
            $("#group-modal-title").text(button.text());

            let i: number;
            for (i = 1; ; i++) {
                let drug = button.data(`drug${i - 1}`) as Drug;
                if (drug === undefined)
                    break;
                this.setupDrugRowForGroupDefModal(rows.eq(i), drug);
            }
            for (; i < rows.length; i++) {
                this.setupDrugRowForGroupDefModal(rows.eq(i));
            }
        });
    }

    private setupPvalueInteractionForResults(table: JQuery<HTMLTableElement>) {
        let trs = table.find("tr");
        for (let i = 1; i < trs.length; i++) {
            let tds = trs.eq(i).find("td");
            for (let j = 0; j < tds.length; j++) {
                let td = tds.eq(j);

                let gpv = td.data("gpvalues") as [number, number][];
                if (gpv !== undefined && gpv.length !== 0) {
                    td.mouseenter(ev => {
                        td.addClass("active").attr("data-tag", "differ significantly");
                        for (let [id, pv] of gpv) {
                            tds.eq(id).addClass("active").attr("data-tag", `when p < ${pv}`);
                        }
                    });
                    td.mouseleave(ev => {
                        td.removeClass("active");
                        for (let [id, pv] of gpv) {
                            tds.eq(id).removeClass("active").removeAttr("data-tag");
                        }
                    });
                }

                let ipv = td.data("ipvalues") as [number, number][];
                if (ipv !== undefined && ipv.length !== 0) {
                    td.mouseenter(ev => {
                        td.addClass("active").attr("data-tag", "differ significantly");
                        for (let [id, pv] of ipv) {
                            trs.eq(id).find("td").eq(j).addClass("active").attr("data-tag", `when p < ${pv}`);
                        }
                    });
                    td.mouseleave(ev => {
                        td.removeClass("active").removeAttr("data-tag");
                        for (let [id, pv] of ipv) {
                            trs.eq(id).find("td").eq(j).removeClass("active").removeAttr("data-tag");
                        }
                    });
                }
            }
        }
    }

    private setupDrugRowForGroupDefModal(row: JQuery<HTMLTableRowElement>, drug?: Drug) {
        const tds = row.children("td");

        if (drug !== undefined) {
            tds.parent().show();
            tds.eq(0).text(drug.treatment as string);
            tds.eq(0).attr("title", drug.treatment_full as string);

            if (typeof (drug.drug) === "string")
                tds.eq(1).text(drug.drug);
            else
                tds.eq(1).text("");

            if (typeof (drug.dose) === "number") {
                tds.eq(2).text(`${drug.dose} ${drug.unit}`);
                tds.eq(2).attr("title", drug.unit_full as string);
            }
            else {
                tds.eq(2).text("");
                tds.eq(2).attr("title", "");
            }

            if (typeof (drug.admin) === "string") {
                tds.eq(3).text(drug.admin);
                tds.eq(3).attr("title", drug.admin_full as string);
            }
            else {
                tds.eq(3).text("");
                tds.eq(3).attr("title", "");
            }
        }
        else {
            tds.parent().hide();
        }
    }

    private loadSection(sectionSelector: string, id?: string, id2?: string, smiles?: string, onload?: (container: JQuery<HTMLElement>) => void) {
        let section = $(sectionSelector);
        let link = section.children(".action-link");
        let href = (link.attr("href") as string).replace(/\/0$/, `/${id}`).replace(/\/0\//, `/${id2}/`);
        if (id === undefined && smiles !== undefined)
            href = href.replace(/\/undefined$/, `?smiles=${encodeURI(smiles)}`);
        let container = link.data("container") as string;

        $.get(href).done(d => {
            $(`#sidebar .nav a[href='${sectionSelector}']`).removeClass("disabled");
            $(container).html(d);
            if (onload !== undefined)
                onload($(container));
        });
    }

    private selectOption(btn: JQuery<HTMLElement>, activeSelector?: string) {
        if (btn.hasClass("disabled") || btn.prop("disabled"))
            return false;
        if (activeSelector !== undefined) {
            let marker = btn.parents(activeSelector);
            if (marker.hasClass("disabled") || marker.prop("disabled"))
                return false;
            if (marker.hasClass("active"))
                return false;
            btn.parents(activeSelector).addClass("active").siblings(".active").removeClass("active");
        }
        else {
            if (btn.hasClass("active"))
                return false;
            btn.addClass("active").siblings(".active").removeClass("active");
        }
        return true;
    }

    private loadForTreatment(drugId: string | undefined, subjectId: string, smiles?: string) {
        // load models section
        this.loadSection("#models", drugId, subjectId, smiles);

        // load simulation section
        this.loadSection("#simulation", drugId, subjectId, smiles, container => {
            if (this.animationManager !== undefined)
                this.animationManager.setGroups(container.find("#scene1").data("groups"));
        });

        // load results section
        this.loadSection("#results", drugId, subjectId, smiles, container => {
            // setup bar chart modal
            this.setupBarChartModalForResults(container.parent().find("#chart-modal"));

            // setup group definition modal
            this.setupGroupDefModalForResults(container.parent().find("#group-modal"));

            // setup pvalue interaction
            let tables = container.find("table") as JQuery<HTMLTableElement>;
            for (let i = 0; i < tables.length; i++)
                this.setupPvalueInteractionForResults(tables.eq(i));
        });
    }

    start() {
        // load subject section
        this.loadSection("#subject", undefined, undefined, undefined, subject => {
            //$("nav .nav .nav-link[href!='#subject']").addClass("disabled");

            subject.find(".list-group-item").click(event => {
                let target = $(event.currentTarget);
                let tag = target.children("span").text();
                $(".nav .nav-link[href='#subject']").attr("data-tag", tag).parent().attr("data-tag", tag);
                $(".nav .nav-link[href='#treatment']").removeAttr("data-tag").parent().removeAttr("data-tag");

                if (this.selectOption(target)) {
                    // load treatment section
                    let subjectId = target.data("subject-id") as string;
                    this.loadSection("#treatment", subjectId, undefined, undefined, treatment => {
                        treatment.find(".fn-select-card").click(event2 => {
                            let card = $(event2.currentTarget);

                            if (this.selectOption(card)) {
                                // show tag on menu
                                let tag2 = card.children(".card-footer").text();
                                $(".nav .nav-link[href='#treatment']").attr("data-tag", tag2).parent().attr("data-tag", tag2);
                                // load for smiles
                                let btn = card.find("a[data-toggle=modal]");
                                this.loadForTreatment(card.data("drugid"), subjectId, btn.data("smiles"));
                            }
                        });

                        let modal = treatment.parent().find("#meditor-modal");
                        modal.find(".fn-save-drug").click(event2 => {
                            if (this.jsmeApplet !== undefined) {
                                const smiles = this.jsmeApplet.smiles();
                                // validate input
                                if (smiles === undefined || smiles === "") {
                                    alert("Molecule cannot be blank");
                                    return;
                                }
                                const card = treatment.find(".card:last-of-type");
                                const btn = card.find("a[data-toggle=modal]");
                                let tag2 = card.children(".card-footer").text();
                                $(".nav .nav-link[href='#treatment']").attr("data-tag", tag2).parent().attr("data-tag", tag2);

                                btn.data("smiles", smiles);
                                card.removeClass("disabled");

                                this.selectOption(card);
                                this.loadForTreatment(undefined, subjectId, smiles);
                                modal.modal("hide");
                            }
                        });
                    });
                }
            });
        });
    }

    private updateUrlHash(hash: string) {
        if (window.location.hash !== hash)
            history.replaceState({}, "", hash);
    }

    private smoothScroll(event: JQuery.ClickEvent<HTMLElement, null, HTMLElement, HTMLElement>) {
        const hash = $(event.delegateTarget).attr("href") as string;
        const offset = $(hash === "" ? "html" : hash).offset();

        if (offset !== undefined) {
            $("html,body").animate({
                scrollTop: offset.top
            }, () => {
                this.updateUrlHash(hash);
            });
            return false;
        }
        return true;
    }

    private onSectionChange(app: AnimalLabApp, section: keyof Sections | undefined, selected: boolean) {
        switch (section) {
            case undefined:
                break;
            case "subject":
                break;
            case "treatment":
                break;
            case "models":
                break;
            case "simulation":
                if (app.animationManager === undefined)
                    break;
                if (selected)
                    app.animationManager.startAnimation();
                else
                    app.animationManager.pauseAnimation();
                break;
            case "results":
                break;
        }
    }

    setupNavigation() {
        // nav menu => scroll position
        $("a[href^='#']").gatedClick(event => this.smoothScroll(event));
        $("a[href='']").gatedClick(event => this.smoothScroll(event));

        // synchronize scroll position with nav menu selection, url hash and next-step button
        $(window).scroll(event => {
            // locate the section that is currently shown
            // undefined sectionId if no section is shown
            let sectionId: keyof Sections | undefined;
            let scrollTop = $(window).scrollTop() as number;

            $(".body > section").each(function () {
                let sectionTop = ($(this).offset() as JQuery.Coordinates).top;
                if (scrollTop >= Math.floor(sectionTop))
                    sectionId = $(this).attr("id") as keyof Sections;
            });

            // fetch current selection
            const oldActiveA = $(".sidebar > ol > li > a.active");
            let oldSectionId: keyof Sections | undefined;

            if (oldActiveA.length > 0) {
                oldSectionId = (oldActiveA.attr("href") as string).substr(1) as keyof Sections;
            }

            // no section shown, being top of the page
            if (sectionId === undefined) {
                // [1] no selection
                if (oldSectionId !== undefined) {
                    oldActiveA.removeClass("active");
                    this.onSectionChange(this, oldSectionId, false);
                }

                $(`.sidebar > ol > li > a[href='']`).addClass("active");

                // [2] no hash
                this.updateUrlHash("");

                // [3] no next-step button
                let nextId = $("section").eq(0).attr("id");
                $("#next-step").attr("href", `#${nextId}`).fadeIn();

                this.onSectionChange(this, sectionId, true);
                return;
            }

            // section changed
            if (oldSectionId !== sectionId) {
                // [1] update selection
                if (oldSectionId !== undefined) {
                    oldActiveA.removeClass("active");
                    this.onSectionChange(this, oldSectionId, false);
                }

                $(`.sidebar > ol > li > a[href='#${sectionId}']`).addClass("active");

                // [2] update url hash for selected menu item
                this.updateUrlHash(`#${sectionId}`);

                // [3] update next step button status
                let nextId = $(`#${sectionId}`).next("section").attr("id");
                if (nextId !== undefined)
                    $("#next-step").attr("href", `#${nextId}`).fadeIn();
                else
                    $("#next-step").fadeOut();

                this.onSectionChange(this, sectionId, true);
            }
        });

        $(window).scroll();
    }

    onMolEditorLoad() {
        this.jsmeApplet = new JSApplet.JSME("jsme", "600px", "600px", {
            "options": "nozoom,noshowdraganddropsymbolindepictmode",
            "jme": "",
        });

        // triggered right after toggle button clicked
        $("#meditor-modal").on("show.bs.modal", event => {
            if (this.jsmeApplet !== undefined) {
                this.jsmeApplet.clear();
                $("#span-loading").show();
                this.jsmeApplet.setWidth("0");
            }
        });

        // triggered after modal being shown
        $("#meditor-modal").on("shown.bs.modal", event => {
            if (this.jsmeApplet !== undefined) {
                this.jsmeApplet.setWidth("100%");

                let button = $(event.relatedTarget as HTMLElement);

                let smiles = button.data("smiles");
                if (smiles !== undefined) {
                    this.jsmeApplet.readGenericMolecularInput(smiles);
                }
                else {
                    this.jsmeApplet.clear();
                    this.jsmeApplet.repaint();
                }

                $("#span-loading").hide();
            }
        });
    }
}