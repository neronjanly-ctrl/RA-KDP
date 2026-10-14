class TalkingCloudRenderer {
    canvasSize: Size;
    maskOpacity: number;
    pathElem: SVGPathElement;
    textElem: SVGTextElement;

    /**
     * Create a talking cloud renderer from a canvas size and a containing <g> element.
     * @param canvasSize The size of the canvas that the talking cloud is rendered upon.
     * @param gSelector A <g> element containing a path element and a text element.
     */
    constructor(canvasSize: Size, maskOpacity: number, gSelector: string);

    /**
     * Create a talking cloud renderer from a canvas size, a path element and a text element.
     * @param canvasSize The size of the canvas that the talking cloud is rendered upon.
     * @param pathElem A <path> element that the talking cloud shape is rendered upon.
     * @param textElem A <text> element that the talking cloud message is rendered upon.
     */
    constructor(canvasSize: Size, maskOpacity: number, pathElem: SVGPathElement, textElem: SVGTextElement);

    constructor(canvasSize: Size, maskOpacity: number, pathElemOrSelector: SVGPathElement | string, textElem?: SVGTextElement) {
        this.canvasSize = canvasSize;
        this.maskOpacity = maskOpacity;

        if (textElem !== undefined) {
            this.pathElem = pathElemOrSelector as SVGPathElement;
            this.textElem = textElem;
        }
        else {
            let g = d3.select<SVGGElement, {}>(pathElemOrSelector as string);
            this.pathElem = g.select("path").node() as SVGPathElement;
            this.textElem = g.select("text").node() as SVGTextElement;
        }
    }

    /**
     * @param canvasSize The size of the entire canvas.
     * @param pathElem A <path> element that the highlight mask is being built upon.
     * @param textElem A <text> element that the message is being shown upon.
     * @param msg The message that is being shown on the tip of the highlighter.
     * @param clipRect A rectangle of the highlighted area.
     */
    draw(clipRect: Rect, msg: string) {
        const margin = 10;
        const cue: number = (clipRect.center.x < this.canvasSize.width / 2 ? 0 : 2) + (clipRect.center.y < this.canvasSize.height / 2 ? 0 : 1); // 0: top-left, 1: bottom-left, 2: top-right, 3: bottom-right
        const tipDist = 30, tipOffset = 10;
        const baseWidth = 20, baseOffset = 10;
        const cueRadiusIn = 60, cueRadiusOut = 120;
        const tipsFontSize = 20;

        let tipX = clipRect.center.x, tipY = clipRect.center.y;
        let tipsAnchor: string = "";

        let d = `M0,0 h${this.canvasSize.width} v${this.canvasSize.height} h-${this.canvasSize.width}z M${clipRect.left},${clipRect.top - margin}`;

        if (cue === 1) {
            d += `h${clipRect.width - baseWidth - baseOffset}a${cueRadiusOut},${cueRadiusOut},0,0,1,${baseWidth + tipOffset},${-tipDist}a${cueRadiusIn},${cueRadiusIn},0,0,0,${-tipOffset},${tipDist}h${baseOffset}`;
            tipX = clipRect.right - baseOffset + tipOffset;
            tipY = clipRect.top - margin - tipDist - tipsFontSize / 2;
            tipsAnchor = "middle";
        }
        else {
            d += `h${clipRect.width}`;
        }
        d += `a${margin},${margin},0,0,1,${margin},${margin}`;

        if (cue == 0) {
            d += `v${clipRect.height - baseWidth - baseOffset}a${cueRadiusOut},${cueRadiusOut},0,0,1,${tipDist},${baseWidth + tipOffset}a${cueRadiusIn},${cueRadiusIn},0,0,0,${-tipDist},${-tipOffset}v${baseOffset}`;
            tipX = clipRect.right + margin + tipDist / 2;
            tipY = clipRect.bottom - baseOffset + tipOffset + tipsFontSize;
            tipsAnchor = "start";
        }
        else {
            d += `v${clipRect.height}`;
        }
        d += `a${margin},${margin},0,0,1,${-margin},${+margin}`;

        if (cue == 2) {
            d += `h${-clipRect.width + baseWidth + baseOffset}a${cueRadiusOut},${cueRadiusOut},0,0,1,${-baseWidth - tipOffset},${tipDist}a${cueRadiusIn},${cueRadiusIn},0,0,0,${tipOffset},${-tipDist}h${-baseOffset}`;
            tipX = clipRect.left + baseOffset - tipOffset;
            tipY = clipRect.bottom + margin + tipDist + tipsFontSize;
            tipsAnchor = "middle";
        }
        else {
            d += `h${-clipRect.width}`;
        }
        d += `a${margin},${margin},0,0,1,${-margin},${-margin}`;

        if (cue == 3) {
            d += `v${-clipRect.height + baseWidth + baseOffset}a${cueRadiusOut},${cueRadiusOut},0,0,1,${-tipDist},${-baseWidth - tipOffset}a${cueRadiusIn},${cueRadiusIn},0,0,0,${tipDist},${tipOffset}v${-baseOffset}`;
            tipX = clipRect.left - margin - tipDist / 2;
            tipY = clipRect.top + baseOffset - tipOffset - tipsFontSize / 2;
            tipsAnchor = "end";
        }
        else {
            d += `v${-clipRect.height}`;
        }
        d += `a${margin},${margin},0,0,1,${+margin},${-margin}z`;

        d3.select(this.pathElem)
            .attr("opacity", this.maskOpacity)
            .attr("d", d);

        d3.select(this.textElem)
            .attr("x", tipX)
            .attr("y", tipY)
            .attr("font-size", tipsFontSize)
            .attr("text-anchor", tipsAnchor)
            .text(msg);
    }

    hide() {
        d3.select(this.pathElem).attr("d", "M0,0");
        d3.select(this.textElem).text("");
    }
}