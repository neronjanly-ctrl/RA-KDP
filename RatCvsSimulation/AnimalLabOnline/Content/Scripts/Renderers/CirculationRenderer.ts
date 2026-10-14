class CirculationRenderer {
    private readonly graph: BezierGraph;

    showOutlines: boolean = false;
    showGizmos: boolean = false;
    showDrugs: boolean = false;
    showSelectors: boolean = false;
    showInjector: boolean = false;

    drugColor: RgbColor;

    readonly names: string[]; // node labels
    readonly selectors: [string, number, number][]; // node label, node id, connector id
    injectedEntry?: [string, number, number]; // node label, node id, connector id

    hovered: number | null = null;
    active: boolean = false;
    selected: boolean = false;

    constructor(graph: BezierGraph, drugColor: RgbColor, names: string[], selectors: [string, number, number][]) {
        this.names = names;
        this.selectors = selectors;

        this.graph = graph;
        this.drugColor = drugColor;
    }

    inject(node: number) {
        for (let i = 0; i < this.selectors.length; i++)
            if (this.selectors[i][1] === node) {
                this.injectedEntry = this.selectors[i];
                this.showDrugs = true;
                break;
            }
    }

    clearDrugs() {
        this.injectedEntry = undefined;
        this.showDrugs = false;
    }

    hitTest(x: number, y: number): number | null {
        if (!this.showSelectors)
            return null;

        let pt = new Point(x, y);

        for (let [label, id, connector] of this.selectors) {
            let node = this.graph.rects[id];

            if (pt.distance(node.center) < Math.min(node.width, node.height) * 0.5)
                return id;
        }

        return null;
    }

    drawSelectors(ctx: CanvasRenderingContext2D) {
        const settings = Settings.current;

        ctx.save();

        for (let [label, id, connector] of this.selectors) {
            let node = this.graph.rects[id];

            if (this.hovered !== id)
                ctx.fillStyle = settings.selectorCircleColor;
            else
                ctx.fillStyle = this.active ? settings.selectorCircleColorActive : settings.selectorCircleColorHover;

            ctx.beginPath();
            ctx.arc(node.center.x, node.center.y, Math.min(node.width, node.height) * 0.5, 0, Math.PI * 2);
            ctx.closePath();
            ctx.fill();

            ctx.fillStyle = settings.selectorTextColor;
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText(label, node.center.x, node.center.y);
        }

        ctx.restore();
    }

    drawInjector(ctx: CanvasRenderingContext2D, phaseShift: number) {
        if (this.injectedEntry === undefined)
            return;

        const settings = Settings.current;
        let scale = 0.4;

        let position = phaseShift;
        let borderWidth = 2 * scale;
        let scaleWidth = 1 * scale;
        let borderColor = "black";
        let shadowColor = "white";
        let sealColor = "navy";
        let luerColor = "navy";
        let graduations = 6;

        let connector = this.graph.connectors[this.injectedEntry[1]][this.injectedEntry[2]];
        let anchor = connector.endPoint, dir = connector.controlPoint;

        ctx.save();

        let angle = anchor.angle(dir);
        ctx.translate(anchor.x, anchor.y);
        ctx.rotate(angle + Math.PI / 2); // rotate clockwise
        ctx.translate(-anchor.x, -anchor.y);

        // Syringe parts ref: https://en.wikipedia.org/wiki/Syringe#/media/File:Labeled_syringe.png
        // https://syringepumppro.com/faq-items/parts-of-the-syringe/
        const barrelWidth = 30 * scale, barrelHeight = 140 * scale, luerWidth = 10 * scale, luerHeight = 40 * scale, shaftWidth = 3 * scale, shaftHeight = 40 * scale;
        const sealHeight = 10 * scale, plungerFlangeHeight = 0 * scale, barrelFlangeHeight = 0 * scale;

        const plungerHeight = barrelHeight + plungerFlangeHeight + borderWidth;
        const plungerFlangeWidth = barrelWidth * 2, plungerWidth = barrelWidth * 0.7;
        const barrelFlangeWidth = barrelWidth * 2.6;
        const sealWidth = barrelWidth, plungerBodyHeight = plungerHeight - plungerFlangeHeight - sealHeight;
        const barrelBodyHeight = barrelHeight - barrelFlangeHeight;

        // draw needle shaft
        ctx.beginPath();
        ctx.moveTo(anchor.x, anchor.y);
        ctx.lineTo(anchor.x, anchor.y - shaftHeight);
        ctx.lineWidth = shaftWidth;
        ctx.strokeStyle = borderColor;
        ctx.stroke();

        // draw luer
        const luerRect = Rect.fromCenterSize(anchor.x, anchor.y - shaftHeight - luerHeight / 2, luerWidth, luerHeight);

        ctx.fillStyle = luerColor;
        ctx.fillRect(luerRect.left, luerRect.top, luerRect.width, luerRect.height);

        // draw plunger
        const minPos = luerRect.top, maxPos = minPos - barrelHeight + barrelFlangeHeight + sealHeight;
        const curPos = minPos * (1 - position) + maxPos * position;

        const plungerFlangeRect = Rect.fromCenterSize(anchor.x, curPos - plungerHeight + plungerFlangeHeight / 2, plungerFlangeWidth, plungerFlangeHeight);
        const plungerBodyRect = Rect.fromCenterSize(anchor.x, plungerFlangeRect.bottom + plungerBodyHeight / 2, plungerWidth, plungerBodyHeight);

        ctx.beginPath();
        ctx.moveTo(plungerFlangeRect.left, plungerFlangeRect.bottom); // plunger head
        ctx.lineTo(plungerFlangeRect.left, plungerFlangeRect.top);
        ctx.lineTo(plungerFlangeRect.right, plungerFlangeRect.top);
        ctx.lineTo(plungerFlangeRect.right, plungerFlangeRect.bottom);
        ctx.lineTo(plungerBodyRect.right, plungerBodyRect.top); // plunger body
        ctx.lineTo(plungerBodyRect.right, plungerBodyRect.bottom);
        ctx.lineTo(plungerBodyRect.left, plungerBodyRect.bottom);
        ctx.lineTo(plungerBodyRect.left, plungerBodyRect.top);
        ctx.closePath();

        ctx.lineWidth = borderWidth;
        ctx.strokeStyle = borderColor;
        ctx.stroke();

        // draw drug
        const drugRect = Rect.fromCenterSize(anchor.x, (minPos + curPos - borderWidth) / 2, barrelWidth, curPos - minPos);

        ctx.fillStyle = settings.drugColor.toHtml();
        ctx.fillRect(drugRect.left, drugRect.top, drugRect.width, drugRect.height);

        // draw seal
        const sealRect = Rect.fromCenterSize(anchor.x, curPos - sealHeight / 2 - borderWidth / 2, sealWidth, sealHeight);
        ctx.fillStyle = sealColor;
        ctx.fillRect(sealRect.left, sealRect.top, sealRect.width, sealRect.height);

        // draw scales
        const graduationHeight = minPos - maxPos, stepLength = (graduationHeight / (graduations + 2)) | 0;
        const colors = [borderColor, shadowColor];

        ctx.lineWidth = scaleWidth;
        for (let j = 0; j < 2; j++) {
            ctx.beginPath();
            const offsetY = luerRect.top + j * scaleWidth;
            for (let i = 0; i < graduations; i += 2) {
                ctx.moveTo(drugRect.left, offsetY - (i + 1) * stepLength);
                ctx.lineTo(drugRect.left + barrelWidth / 3, offsetY - (i + 1) * stepLength);
                ctx.moveTo(drugRect.left, offsetY - (i + 2) * stepLength);
                ctx.lineTo(drugRect.left + barrelWidth * 2 / 3, offsetY - (i + 2) * stepLength);
            }
            ctx.strokeStyle = colors[j];
            ctx.stroke();
        }

        // draw barrel
        const barrelFlangeRect = Rect.fromCenterSize(anchor.x, luerRect.top - barrelHeight + barrelFlangeHeight / 2, barrelFlangeWidth, barrelFlangeHeight);
        const barrelBodyRect = Rect.fromCenterSize(anchor.x, barrelFlangeRect.bottom + barrelBodyHeight / 2, barrelWidth, barrelBodyHeight);

        ctx.beginPath();
        ctx.moveTo(barrelFlangeRect.left, barrelFlangeRect.bottom); // barrel flange
        ctx.lineTo(barrelFlangeRect.left, barrelFlangeRect.top);
        ctx.lineTo(barrelFlangeRect.right, barrelFlangeRect.top);
        ctx.lineTo(barrelFlangeRect.right, barrelFlangeRect.bottom);
        ctx.lineTo(barrelBodyRect.right, barrelBodyRect.top); // barrel body
        ctx.lineTo(barrelBodyRect.right, barrelBodyRect.bottom);
        ctx.lineTo(barrelBodyRect.left, barrelBodyRect.bottom);
        ctx.lineTo(barrelBodyRect.left, barrelBodyRect.top);
        ctx.closePath();

        ctx.lineWidth = borderWidth;
        ctx.strokeStyle = borderColor;
        ctx.stroke();

        ctx.restore();
    }

    draw(canvas: HTMLCanvasElement, phaseShift: number, rounds: number): void {
        let ctx = canvas.getContext("2d");
        if (!ctx)
            return;

        let settings = Settings.current;

        ctx.save();

        ctx.scale(window.devicePixelRatio, window.devicePixelRatio);
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        this.graph.drawDots(ctx, phaseShift);

        if (this.showOutlines)
            this.graph.drawOutlines(ctx);

        if (this.showGizmos)
            this.graph.drawGizmos(ctx);

        if (this.showDrugs && this.injectedEntry !== undefined) {
            ctx.fillStyle = this.drugColor.toHtml();
            this.graph.drawTraceDots(ctx, this.injectedEntry[1], phaseShift, rounds);
        }

        if (this.showSelectors)
            this.drawSelectors(ctx);

        if (this.showInjector && rounds < settings.injectCycles * settings.injectRounds)
            this.drawInjector(ctx, (settings.injectCycles - 1 - rounds % settings.injectCycles + (1 - phaseShift)) / settings.injectCycles);

        ctx.restore();
    }
}