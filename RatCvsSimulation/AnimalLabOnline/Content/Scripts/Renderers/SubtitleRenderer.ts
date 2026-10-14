class SubtitleRenderer {
    canvasSize: Size;
    maskOpacity: number;
    rectElem: SVGRectElement;
    textElem: SVGTextElement;

    constructor(canvasSize: Size, maskOpacity: number, gSelector: string);
    constructor(canvasSize: Size, maskOpacity: number, rectElem: SVGRectElement, textElem: SVGTextElement);

    constructor(canvasSize: Size, maskOpacity: number, rectElemOrSelector: SVGRectElement | string, textElem?: SVGTextElement) {
        this.canvasSize = canvasSize;
        this.maskOpacity = maskOpacity;

        if (textElem !== undefined) {
            this.rectElem = rectElemOrSelector as SVGRectElement;
            this.textElem = textElem;
        }
        else {
            let g = d3.select<SVGGElement, {}>(rectElemOrSelector as string);
            this.rectElem = g.select("rect").node() as SVGRectElement;
            this.textElem = g.select("text").node() as SVGTextElement;
        }
    }

    show(msg: string) {
        d3.select(this.rectElem)
            .attr("opacity", this.maskOpacity);

        d3.select(this.textElem)
            .attr("opacity", 1)
            .text(msg);
    }

    hide() {
        d3.select(this.rectElem)
            .attr("opacity", 0);

        d3.select(this.textElem)
            .attr("opacity", 0)
            .text("");
    }

    splash(msg: string, durationShown: number): void;
    splash(msg: string, durationShown: number, durationFadeOut: number): void;
    splash(msg: string, durationShown: number, durationFadeIn: number, durationFadeOut: number): void;

    splash(msg: string, durationShown: number, arg3?: number, arg4?: number): void {
        let r = d3.select(this.rectElem);
        let t = d3.select(this.textElem).text("");

        t.selectAll("tspan").remove();

        if (msg.includes("\n")) {
            let lines = msg.split("\n");
            for (let i = 0; i < lines.length; i++) {
                if (i == 0)
                    t.append("tspan").text(lines[i]);
                else
                    t.append("tspan").text(lines[i]).attr("x", "50%").attr("dy", 72);
            }
        }
        else {
            t.text(msg);
        }

        if (arg3 === undefined) {
            r.attr("opacity", this.maskOpacity)
                .transition()
                .duration(0)
                .delay(durationShown)
                .attr("opacity", 0);

            t.attr("opacity", 1)
                .transition()
                .duration(0)
                .delay(durationShown)
                .attr("opacity", 0);
        }
        else if (arg4 === undefined) {
            r.attr("opacity", this.maskOpacity)
                .transition()
                .delay(durationShown)
                .duration(arg3)
                .attr("opacity", 0);

            t.attr("opacity", 1)
                .transition()
                .delay(durationShown)
                .duration(arg3)
                .attr("opacity", 0);
        }
        else {
            r.transition()
                .duration(arg3)
                .attr("opacity", this.maskOpacity)
                .transition()
                .delay(durationShown)
                .duration(arg4)
                .attr("opacity", 0);

            t.transition()
                .duration(arg3)
                .attr("opacity", 1)
                .transition()
                .delay(durationShown)
                .duration(arg4)
                .attr("opacity", 0);
        }
    }

    fadeIn(msg: string, duration: number) {
        d3.select(this.rectElem)
            .transition()
            .duration(duration)
            .attr("opacity", this.maskOpacity);

        d3.select(this.textElem)
            .text(msg)
            .transition()
            .duration(duration)
            .attr("opacity", 1);
    }

    fadeOut(duration: number) {
        d3.select(this.rectElem)
            .transition()
            .duration(duration)
            .attr("opacity", 0);

        d3.select(this.textElem)
            .transition()
            .duration(duration)
            .attr("opacity", 0);
    }
}