class AnimationManager {
    private circulation?: Circulation;
    private assistant: JQuery<HTMLElement>;
    private assistantSvg?: AssistantRenderer;
    private assistHeight: number;
    private readonly therapyDrug: DrugProtocol;
    private readonly inducerDrug: DrugProtocol;

    constructor(names: string[], nodes: number[][], edges: number[][], selectors: [string, number, number][], therapyDrug: DrugProtocol, inducerDrug: DrugProtocol) {
        // circulation animation
        const jcc = $("canvas.circulation");
        const logger = new Logger($("#logs-output"));

        this.therapyDrug = therapyDrug;
        this.inducerDrug = inducerDrug;

        if (jcc.length > 0) {
            this.circulation = new Circulation(logger, jcc, names, nodes, edges, selectors);

            $(".fn-clear-drug").gatedClick(event => {
                if (this.circulation !== undefined)
                    this.circulation.clearDrugs();
            });

            $(".fn-inject-drug").gatedClick(event => {
                if (this.circulation !== undefined)
                    this.circulation.beginSelection();
            });

            $(".fn-pause-simulation").gatedClick(event => {
                this.toggleLoop();
            });

            $(".fn-toggle-gizmos").gatedClick(event => {
                if (this.circulation !== undefined)
                    this.circulation.showGizmos = !this.circulation.showGizmos;
            });

            $(".fn-toggle-outlines").gatedClick(event => {
                if (this.circulation !== undefined)
                    this.circulation.showOutlines = !this.circulation.showOutlines;
            });

            $(".fn-toggle-direction").gatedClick(event => {
                if (this.circulation !== undefined)
                    this.circulation.reversed = !this.circulation.reversed;
            });
        }

        // assistant animation
        this.assistant = $("#assistant");
        this.assistHeight = this.assistant.height() as number;

        this.assistantSvg = new AssistantRenderer("#assistant-svg");
        this.assistantSvg.draw("deeppink");
    }

    private animationLoop() {
        if (!this.running) return;
        this.animationHandle = window.requestAnimationFrame(this.animationLoop.bind(this));

        if (this.circulation !== undefined) {
            this.circulation.tick();
            this.circulation.invalidate();
        }

        if (this.assistantSvg !== undefined) {
            const hand = new Point(50 + 10 * Math.cos(this.arc), 70 + 20 * Math.sin(this.arc)); // hand coordinate
            this.assistantSvg.moveStick(hand);
            this.arc += Math.PI / 180 * 8;
        }
    }

    private toggleLoop() {
        this.running = !this.running;
        if (this.running) {
            this.animationLoop();
            $(".fn-pause-simulation i").removeClass("fa-play-circle").addClass("fa-pause-circle");
            $(".fn-pause-simulation").siblings(".disabled-group").removeClass("disabled");
        }
        else {
            $(".fn-pause-simulation i").removeClass("fa-pause-circle").addClass("fa-play-circle");
            $(".fn-pause-simulation").siblings(".disabled-group").addClass("disabled");
        }
    }

    private running = true;
    private animationHandle?: number;
    private arc = 0;
    private playingScene = 0;

    private renderScene1() {
        if (this.groupNames === undefined)
            return;

        this.playingScene = 1;

        $("#scene1").show();
        $("#scene2").hide();
        $("#scene3").hide();

        let rect = Rect.fromViewBox(d3.select("#scene1").attr("viewBox"));
        let size = new Size(rect.width, rect.height);

        let talkingCloud = new TalkingCloudRenderer(size, 0.7, "#scene1 .talkingCloud");
        let encaging = new EncagingRenderer(size, this.groupNames, 5, "#scene1 .encaging");
        let cageRect = encaging.getCageRect(0);

        setTimeout(() =>
            talkingCloud.draw(cageRect, "Separate groups in different cages")
            , 2000);

        let ratRect = encaging.getRatRect(0, 0), scaleRect = encaging.getScaleRect(false);
        ratRect.width *= 13;
        setTimeout(() =>
            talkingCloud.draw(ratRect, "Randomly pick rats")
            , 4000);

        setTimeout(() =>
            talkingCloud.draw(scaleRect, "Weigh each rat with a scale")
            , 6000);

        let subtitle = new SubtitleRenderer(size, 0.7, "#scene1 .subtitles");

        setTimeout(() => {
            subtitle.splash("Now weigh and allocate rats\nin group specific cages", 2000, 200);
            talkingCloud.hide();
        }, 8000);

        encaging.draw();
        setTimeout(() => encaging.animate(256, 12), 10000);

        setTimeout(() => {
            subtitle.splash("Next, treat rats regularly\nas per administration protocol", 4000, 200);
        }, 18000);

        setTimeout(() => {
            $("#scene1").hide();
            this.renderScene2();
        }, 22000);
    }

    renderScene2() {
        this.playingScene = 2;
        $("#scene2").show();

        let rect = Rect.fromViewBox(d3.select("#scene2").attr("viewBox"));
        let size = new Size(rect.width, rect.height);

        let subtitle = new SubtitleRenderer(size, 0.7, "#scene2 .subtitles");
        let injection = d3.select("#scene2 .injection");

        injection.selectAll("use").remove();
        let lhX = 600, lhY = 200, dX = 147, dY = 110;
        let lh = injection.append("use")
            .attr("xlink:href", "#ig-lh")
            .attr("x", lhX)
            .attr("y", lhY)
            .attr("opacity", 0);

        let ac = injection.append("use")
            .attr("xlink:href", "#ig-ac")
            .attr("x", lhX + dX)
            .attr("y", lhY + dY)
            .attr("opacity", 0);

        let rh = injection.append("use")
            .attr("xlink:href", "#ig-rh")
            .attr("x", 100)
            .attr("y", 50)
            .attr("opacity", 0);

        let splashDelay = 3000, moveDuration = 3000, acDuration = 1000, injectDuration = 1000, time = 0;

        subtitle.splash(`${this.therapyDrug.name} is administered\nvia ${this.therapyDrug.admin}`, splashDelay, 200);
        time += splashDelay;

        rh.transition()
            .delay(splashDelay)
            .duration(1000)
            .attr("opacity", 1);
        time += 2000;

        setTimeout(() => {
            subtitle.splash(`For each rat, prepare ${this.therapyDrug.name}\nat ${this.therapyDrug.dose} body weight`, splashDelay, 200);

            lh.transition()
                .delay(splashDelay)
                .duration(1000)
                .attr("opacity", 1);
        }, time);

        time += splashDelay + 2000;

        let targetX = 596, targetY = -440;

        setTimeout(() => {
            subtitle.splash("Hold the rat tight, move needle tip\ntoward the mouth cavity", splashDelay, 200);

            rh.transition()
                .ease(o => d3.easeSinOut(o))
                .delay(splashDelay)
                .duration(moveDuration)
                .attr("x", targetX)
                .attr("y", targetY);
        }, time);

        time += splashDelay + moveDuration + 1500;

        let injectDepth = 220;

        setTimeout(() => {
            subtitle.splash("Move needle tip toward the mouth cavity", splashDelay, 200);

            ac.transition()
                .ease(o => d3.easeLinear(o))
                .delay(splashDelay)
                .duration(acDuration)
                .attr("opacity", 1);

            rh.transition()
                .ease(o => d3.easeLinear(o))
                .delay(splashDelay + acDuration * 0.8)
                .duration(injectDuration)
                .attr("x", targetX)
                .attr("y", targetY + injectDepth);
        }, time);

        time += splashDelay + acDuration * 0.8 + injectDuration + 1500;

        setTimeout(() => {
            subtitle.splash("Push plunger flange to inject", splashDelay, 200);
        }, time);

        time += splashDelay + 2000;

        setTimeout(() => {
            subtitle.splash("Repeat at an interval of 24h\nfor two consecutive days", splashDelay, 200);
        }, time);

        time += splashDelay + 2000;

        setTimeout(() => {
            $("#scene2").hide();
            this.renderScene3();
        }, time);
    }

    private renderScene3() {
        this.playingScene = 3;

        $("#scene3").show();
        let rect = Rect.fromViewBox(d3.select("#scene3").attr("viewBox"));
        let size = new Size(rect.width, rect.height);

        let subtitle = new SubtitleRenderer(size, 0.7, "#scene3 .subtitles");
        let injection = d3.select("#scene3 .injection");

        injection.selectAll("use").remove();
        let lh = injection.append("use")
            .attr("xlink:href", "#ip-lh")
            .attr("x", 600)
            .attr("y", 200)
            .attr("opacity", 0);

        let rh = injection.append("use")
            .attr("xlink:href", "#ip-rh")
            .attr("x", 100)
            .attr("y", 100)
            .attr("clip-path", "url(#needle-mask-ip)")
            .attr("opacity", 0);

        let nm = d3.select("#scene3 #needle-mask-ip rect")
            .attr("x", 0);

        let splashDelay = 3000, moveDuration = 3000, injectDuration = 1000, time = 0;

        subtitle.splash(`Next, ${this.inducerDrug.name} is administered\nvia ${this.inducerDrug.admin}`, splashDelay, 200);
        time += splashDelay;

        rh.transition()
            .delay(splashDelay)
            .duration(1000)
            .attr("opacity", 1);
        time += 2000;

        setTimeout(() => {
            subtitle.splash(`For each rat, prepare ${this.inducerDrug.name}\nat ${this.inducerDrug.dose} body weight`, splashDelay, 200);

            lh.transition()
                .delay(splashDelay)
                .duration(1000)
                .attr("opacity", 1);
        }, time);

        time += splashDelay + 2000;

        let targetX = 460, targetY = 355;

        setTimeout(() => {
            subtitle.splash("Hold the rat tight, move needle tip\ntoward the body cavity", splashDelay, 200);

            rh.transition()
                .ease(o => d3.easeSinOut(o))
                .delay(splashDelay)
                .duration(moveDuration)
                .attr("x", targetX)
                .attr("y", targetY);
        }, time);

        time += splashDelay + moveDuration + 2000;

        let injectDepth = 70, tilt = 13 / 180 * Math.PI;

        setTimeout(() => {
            subtitle.splash("Push plunger flange to inject", splashDelay, 200);

            rh.transition()
                .ease(o => d3.easeLinear(o))
                .delay(splashDelay)
                .duration(injectDuration)
                .attr("x", targetX + Math.cos(tilt) * injectDepth)
                .attr("y", targetY - Math.sin(tilt) * injectDepth);

            nm.transition()
                .ease(o => d3.easeLinear(o))
                .delay(splashDelay)
                .duration(injectDuration)
                .attr("x", -Math.cos(tilt) * injectDepth);
        }, time);

        time += splashDelay + injectDuration + 2000;

        setTimeout(() => {
            subtitle.splash("Repeat at an interval of 24h\nfor two consecutive days", splashDelay, 200);
        }, time);

        time += splashDelay + 2000;

        setTimeout(() => {
            subtitle.splash("Scroll down to see experimental results", 600 * 1000, 200);
        }, time);
    }

    private groupNames?: string[];

    setGroups(groupNames: string[]) {
        this.groupNames = groupNames;
    }

    startAnimation() {
        this.animationLoop();

        //const hidden = this.assistant.data("hidden");
        //if (hidden === undefined || hidden === true) {
        //    this.assistant.data("hidden", false);
        //    this.assistant.animate({ bottom: 0 }, "fast");
        //}

        if (this.playingScene == 0) {
            this.renderScene1();
        }
    }

    pauseAnimation() {
        if (this.animationHandle !== undefined)
            window.cancelAnimationFrame(this.animationHandle);

        const hidden = this.assistant.data("hidden");
        if (hidden === undefined || hidden === false) {
            this.assistant.data("hidden", true);
            this.assistant.animate({ bottom: -this.assistHeight }, "fast");
        }
    }
}