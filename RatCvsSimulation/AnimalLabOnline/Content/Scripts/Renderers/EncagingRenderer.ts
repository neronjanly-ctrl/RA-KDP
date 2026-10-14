class EncagingRenderer {
    canvasSize: Size;
    groupRats: number;
    groupNames: string[];
    scaleElem: SVGGElement;
    cagesElem: SVGGElement;

    constructor(canvasSize: Size, groupNames: string[], groupRats: number, gSelector: string);
    constructor(canvasSize: Size, groupNames: string[], groupRats: number, scaleElem: SVGGElement, cagesElem: SVGGElement);

    constructor(canvasSize: Size, groupNames: string[], groupRats: number, gSelectorOrScaleElem: string | SVGGElement, cagesElem?: SVGGElement) {
        this.canvasSize = canvasSize;
        this.groupNames = groupNames;
        this.groupRats = groupRats;

        if (cagesElem !== undefined) {
            this.scaleElem = gSelectorOrScaleElem as SVGGElement;
            this.cagesElem = cagesElem;
        }
        else {
            let g = d3.select(gSelectorOrScaleElem as string);
            this.scaleElem = g.select(".scale").node() as SVGGElement;
            this.cagesElem = g.select(".cages").node() as SVGGElement;
        }

        this.rowCages = Math.min(Math.floor((this.canvasSize.width - this.canvasPadding.width * 2 + this.cageDist - this.cageSize) / this.cageDist), 5);
    }

    private scalePos = new Point(50, 260);
    private scaleLedAnchor = new Point(70, 85);
    private scaleHeight = 40;
    private scaleSize = 100;

    private ratSize = 100;
    private cageSize = 200;
    private cageDist = 300;
    private ratInCageRatio = 0.6;

    private canvasPadding = new Size(40, 60);
    private cagePadding = new Size(10, 40);
    private rowCages: number;

    getCagePos(group: number) {
        return new Point(this.canvasPadding.width + group % this.rowCages * this.cageDist,
            this.canvasPadding.height + Math.floor(group / this.rowCages) * this.cageDist * 1.5);
    }

    getCageRect(group: number) {
        return new Rect(this.cageSize, this.cageSize, this.canvasPadding.width + group % this.rowCages * this.cageDist,
            this.canvasPadding.height + Math.floor(group / this.rowCages) * this.cageDist * 1.5);
    }

    getRatPos(group: number, index: number) {
        return new Point(this.canvasPadding.width + this.scalePos.x + (1 + group * this.groupRats + index) * this.ratSize,
            this.canvasPadding.height + this.scalePos.y);
    }

    getRatRect(group: number, index: number) {
        return new Rect(this.ratSize, this.ratSize, this.canvasPadding.width + this.scalePos.x + (1 + group * this.groupRats + index) * this.ratSize,
            this.canvasPadding.height + this.scalePos.y);
    }

    getRatInCagePos(cagePos: Point, index: number, rowRats: number) {
        return new Point(cagePos.x + this.cagePadding.width + index % rowRats * this.ratSize * this.ratInCageRatio,
            cagePos.y + this.cagePadding.height + Math.floor(index / rowRats) * this.ratSize * this.ratInCageRatio);
    }

    getRatInCageRect(cagePos: Point, index: number, rowRats: number) {
        return new Rect(this.ratSize * this.ratInCageRatio, this.ratSize * this.ratInCageRatio,
            cagePos.x + this.cagePadding.width + index % rowRats * this.ratSize * this.ratInCageRatio,
            cagePos.y + this.cagePadding.height + Math.floor(index / rowRats) * this.ratSize * this.ratInCageRatio);
    }

    getScalePos(on: boolean) {
        return new Point(this.canvasPadding.width + this.scalePos.x, this.canvasPadding.height + this.scalePos.y - (on ? this.scaleHeight : 0));
    }

    getScaleRect(on: boolean) {
        return new Rect(this.scaleSize, this.scaleSize, this.canvasPadding.width + this.scalePos.x, this.canvasPadding.height + this.scalePos.y - (on ? this.scaleHeight : 0));
    }

    genWeightReading(ratAvgWeight: number, ratWeightSE: number) {
        return (ratAvgWeight + Math.random() * ratWeightSE * 2 - ratWeightSE).toPrecision(4);
    }

    draw() {
        let scale = d3.select(this.scaleElem);
        let scalePos = this.getScalePos(false);

        scale.append("use")
            .attr("x", scalePos.x)
            .attr("y", scalePos.y)
            .attr("xlink:href", "#scale");

        scale.append("text")
            .attr("x", scalePos.x + this.scaleLedAnchor.x)
            .attr("y", scalePos.y + this.scaleLedAnchor.y)
            .attr("class", "text-segment7")
            .attr("text-anchor", "end")
            .text(0);

        const _this = this;

        d3.select(this.cagesElem)
            .selectAll("g")
            .remove();

        d3.select(this.cagesElem)
            .selectAll("g")
            .data(this.groupNames)
            .enter()
            .append("g")
            .each(function (o, i) { // i: cage number
                let g = d3.select(this);
                let cagePos = _this.getCagePos(i);

                for (let j = 0; j < _this.groupRats; j++) { // j: rat number
                    let ratPos = _this.getRatPos(i, j);
                    g.append("use")
                        .attr("xlink:href", "#rat")
                        .attr("x", ratPos.x)
                        .attr("y", ratPos.y);
                }

                g.append("use")
                    .attr("xlink:href", "#cage")
                    .attr("x", cagePos.x)
                    .attr("y", cagePos.y);

                g.append("text")
                    .attr("x", cagePos.x + _this.cageSize / 2)
                    .attr("y", cagePos.y + _this.cageSize / 10)
                    .attr("text-anchor", "middle")
                    .attr("font-weight", "bold")
                    .text(o);
            });
    }

    private weighingTime = 100; // 100ms per rat
    private encagingSpeed = 500; // 500 pixels per second

    animate(ratAvgWeight: number, ratWeightSE: number) {
        const rowRats = Math.floor((this.groupRats + 1) / 2);
        const scaleText = d3.select(this.scaleElem).select("text");

        const _this = this;
        let scalePos = this.getScalePos(true);

        d3.select(this.cagesElem)
            .selectAll<SVGGElement, string>("g")
            .each(function (o, i) { // i: cage number
                let g = d3.select(this);
                let cagePos = _this.getCagePos(i);

                let uses = g.selectAll("use");

                for (let j = 0; j < _this.groupRats; j++) { // j: rat number
                    scaleText
                        .transition()
                        .delay((i * _this.groupRats + j + 1) * _this.weighingTime * 2)
                        .text(_this.genWeightReading(ratAvgWeight, ratWeightSE));

                    let ratPos = _this.getRatInCagePos(cagePos, j, rowRats);

                    uses.filter((p, k) => k == j)
                        .transition() // move to one-step-before-scale
                        .ease(o => d3.easeLinear(o))
                        .duration((i * _this.groupRats + j) * _this.weighingTime * 2)
                        .attr("x", _this.scalePos.x + _this.ratSize)

                        .transition() // move on to scale
                        .duration(_this.weighingTime)
                        .attr("x", scalePos.x)
                        .attr("y", scalePos.y)

                        .transition() // move into cage
                        .delay(_this.weighingTime)
                        .duration(Math.sqrt(Math.pow(ratPos.x - _this.scalePos.x, 2) + Math.pow(ratPos.y - _this.scalePos.y, 2)) / _this.encagingSpeed * 1000)
                        .attr("x", ratPos.x / _this.ratInCageRatio)
                        .attr("y", ratPos.y / _this.ratInCageRatio)
                        .attr("transform", `scale(${_this.ratInCageRatio})`);
                }
            });
    }

    hide() {
        d3.select(this.scaleElem).selectAll("*").remove();
        d3.select(this.cagesElem).selectAll("*").remove();
    }
}