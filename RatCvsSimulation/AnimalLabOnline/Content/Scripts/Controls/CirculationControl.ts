class Circulation {
    private readonly canvas: HTMLCanvasElement;
    private readonly renderer: CirculationRenderer;

    reversed: boolean = false;
    private time: number = 0;
    private rounds: number = 0;
    private phaseShift: number = 0;
    private ticksPerRound: number = 20;

    injected?: (node: number) => void;
    logger: Logger;

    constructor(logger: Logger, elements: JQuery<HTMLElement>, names: string[], nodes: number[][], edges: number[][], selectors: [string, number, number][]) {
        if (elements.length === 0)
            throw new Error("A container is required");
        this.logger = logger;

        const ctrl = elements[0] as HTMLCanvasElement;
        const jctrl = $(ctrl);

        const width = jctrl.width() as number, height = jctrl.height() as number;

        //console.log(`w:${width}, h:${height}`);

        jctrl.prop("width", width * window.devicePixelRatio);
        jctrl.prop("height", height * window.devicePixelRatio);

        this.canvas = ctrl;

        let settings = Settings.current;
        let colors = [settings.bloodRedColor, settings.bloodBlueColor, settings.bloodPurpleColor];
        let graph = new BezierGraph(nodes, edges, colors);
        //console.log(settings);

        this.renderer = new CirculationRenderer(graph, settings.drugColor, names, selectors);
    }

    tick() {
        this.time++;
        if (this.time >= this.ticksPerRound) {
            this.time = 0;
            if (this.renderer.injectedEntry !== undefined)
                this.rounds++;
        }
        this.phaseShift = this.time / this.ticksPerRound;
        if (this.reversed)
            this.phaseShift = 1 - this.phaseShift;
    }

    invalidate() {
        this.renderer.draw(this.canvas, this.phaseShift, this.rounds);
    }

    get showGizmos() {
        return this.renderer.showGizmos;
    }

    set showGizmos(show: boolean) {
        this.logger.info(`showGizmos: ${show}`);
        this.renderer.showGizmos = show;
    }

    get showOutlines() {
        return this.renderer.showOutlines;
    }

    set showOutlines(show: boolean) {
        this.logger.info(`showOutlines: ${show}`);
        this.renderer.showOutlines = show;
    }

    get showDrugs() {
        return this.renderer.showDrugs;
    }

    set showDrugs(show: boolean) {
        this.logger.info(`showDrugs: ${show}`);
        this.renderer.showDrugs = show;
    }

    beginSelection() {
        this.renderer.showSelectors = true;

        let jcanvas = $(this.canvas);

        jcanvas.mousemove(ev => {
            let id = this.renderer.hitTest(ev.offsetX as number, ev.offsetY as number);
            this.renderer.hovered = id;
        });

        jcanvas.mousedown(ev => {
            let id = this.renderer.hitTest(ev.offsetX as number, ev.offsetY as number);
            this.renderer.active = id !== null;
        });

        jcanvas.mouseup(ev => {
            let id = this.renderer.hitTest(ev.offsetX as number, ev.offsetY as number);
            if (!this.renderer.active && id === null || this.renderer.active && id !== null) {
                this.renderer.hovered = null;
                this.renderer.active = false;
                this.renderer.showSelectors = false;
                if (id !== null)
                    this.inject(id);
            }
        });
    }

    inject(node: number) {
        this.rounds = 0;
        this.renderer.showInjector = true;
        this.renderer.inject(node);
        this.logger.info(`injectAt: ${node}`);
    }

    clearDrugs() {
        this.rounds = 0;
        this.renderer.showInjector = false;
        this.renderer.clearDrugs();
        this.logger.info(`Everything cleared.`);
    }
}