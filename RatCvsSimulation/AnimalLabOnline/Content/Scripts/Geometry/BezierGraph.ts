class BezierGraph {
    // per node data
    readonly rects: Rect[]; // informatic data for drawing outlines
    readonly connectors: CubicBezierPoint[][]; // informatic data for drawing gizmos
    readonly graph: [number, number][][]; // adjacency list with weights
    readonly distCache: number[][]; // dijkstra distance cache

    // per edge data
    readonly curves: CubicBezier[]; // for drawing curves

    // adjacency matrix
    readonly curveMatrix: CubicBezier[][]; // equivalent to curves[i]

    // global data
    readonly totalWeight: number;

    // nodes: [][0 x, 1 y, 2 width, 3 height, 4 ep0.x, 5 ep0.y, 6 cp0.x, 8 cp0.y, 8 ep1.x, 9 ep1.y, 10 cp1.x, 11 cp1.y...]
    // edges: [][0 source, 1 destination, 2 source_connector, 3 destination_connector, 4 edge_color, 5 edge_length]
    constructor(nodes: number[][], edges: number[][], colors: RgbColor[]) {
        this.rects = [];
        this.connectors = [];
        this.graph = [];
        this.distCache = [];

        for (let node of nodes) {
            this.rects.push(Rect.fromCenterSize(node[0], node[1], node[2], node[3]));

            let pts = [];
            for (let j = 4; j < node.length; j += 4)
                pts.push(new CubicBezierPoint(node[j + 0], node[j + 1], node[j + 2], node[j + 3]));
            this.connectors.push(pts);
            this.graph.push([]);
            this.distCache.push([]);
        }

        this.curves = [];
        this.totalWeight = 0;
        this.curveMatrix = [];

        let i = 0;
        for (let [src, dst, scnn, dcnn, color, len] of edges) {
            this.graph[src].push([dst, len]);

            const pt1 = this.connectors[src][scnn];
            const pt2 = this.connectors[dst][dcnn];
            const rgb = colors[color].toArray();

            let points: number[][] = [];
            points.push([pt1.endPoint.x, pt1.endPoint.y, pt1.controlPoint.x, pt1.controlPoint.y, len, ...rgb]);
            points.push([pt2.endPoint.x, pt2.endPoint.y, pt2.controlPoint.x, pt2.controlPoint.y, 0, ...rgb]);

            let bezier = new CubicBezier(points, false);
            this.curves.push(bezier);
            this.totalWeight += bezier.totalWeight;

            // build adjacency matrices
            if (!this.curveMatrix[src])
                this.curveMatrix[src] = [];
            this.curveMatrix[src][dst] = bezier;
        }
    }

    drawOutlines(ctx: CanvasRenderingContext2D) {
        const settings = Settings.current;

        ctx.save();
        ctx.fillStyle = settings.outlineTextColor;
        ctx.strokeStyle = settings.outlineLineColor;
        ctx.lineWidth = settings.outlineLineWidth;

        let i = 0;
        for (let node of this.rects) {
            ctx.beginPath();
            ctx.rect(node.left, node.top, node.width, node.height);
            ctx.closePath();
            ctx.stroke();

            ctx.textAlign = "center";
            ctx.textBaseline = "bottom";
            ctx.fillText(`[${i++}] ${node.center.x},${node.bottom}`, node.center.x, node.bottom);
        }
        ctx.restore();
    }

    drawGizmos(ctx: CanvasRenderingContext2D) {
        const settings = Settings.current;

        ctx.save();
        ctx.fillStyle = settings.gizmoTextColor;
        ctx.strokeStyle = settings.gizmoLineColor;
        ctx.lineWidth = settings.gizmoLineWidth;

        let i = 0;
        for (let pts of this.connectors) {
            let j = 0;
            for (let pt of pts) {
                ctx.beginPath();
                ctx.arc(pt.endPoint.x, pt.endPoint.y, settings.gizmoRadius, 0, Math.PI * 2);
                ctx.closePath();
                ctx.stroke();

                ctx.beginPath();
                ctx.arc(pt.controlPoint.x, pt.controlPoint.y, settings.gizmoRadius, 0, Math.PI * 2);
                ctx.closePath();
                ctx.stroke();

                ctx.beginPath();
                ctx.moveTo(pt.controlPoint.x, pt.controlPoint.y);
                ctx.lineTo(pt.endPoint.x, pt.endPoint.y);
                ctx.closePath();
                ctx.stroke();

                ctx.textAlign = "center";
                ctx.textBaseline = "top";
                ctx.fillText(`[${i}][${j++}] ${pt.endPoint.x},${pt.endPoint.y}`, pt.controlPoint.x, pt.controlPoint.y);
            }
            i++;
        }
        ctx.restore();
    }

    drawDots(ctx: CanvasRenderingContext2D, phaseShift: number) {
        const dotSize = Settings.current.dotSize;

        if (Settings.current.useCircleDots) {
            for (let curve of this.curves) {
                const pts = curve.getShadedPoints(phaseShift, curve.totalWeight);

                for (let pt of pts) {
                    ctx.fillStyle = pt.color.toHtml();
                    ctx.beginPath();
                    ctx.arc(pt.point.x, pt.point.y, dotSize, 0, Math.PI * 2);
                    ctx.closePath();
                    ctx.fill();
                }
            }
        }
        else {
            for (let curve of this.curves) {
                const pts = curve.getShadedPoints(phaseShift, curve.totalWeight);

                for (let pt of pts) {
                    ctx.fillStyle = pt.color.toHtml();
                    ctx.fillRect(pt.point.x - dotSize, pt.point.y - dotSize, dotSize * 2, dotSize * 2);
                }
            }
        }
    }

    drawTraceDots(ctx: CanvasRenderingContext2D, entryNode: number, phaseShift: number, dots: number) {
        const dotSize = Settings.current.traceDotSize;
        phaseShift = (phaseShift + 0.5) % 1;
        const pts = this.getPoints(entryNode, phaseShift, dots);

        if (Settings.current.useCircleDots) {
            for (let pt of pts) {
                pt.x += Math.random() * 3 - 1.5;
                pt.y += Math.random() * 3 - 1.5;

                ctx.beginPath();
                ctx.arc(pt.x, pt.y, dotSize, 0, Math.PI * 2);
                ctx.closePath();
                ctx.fill();
            }
        }
        else {
            for (let pt of pts) {
                pt.x += Math.random() * 3 - 1.5;
                pt.y += Math.random() * 3 - 1.5;

                ctx.fillRect(pt.x - dotSize, pt.y - dotSize, dotSize * 2, dotSize * 2);
            }
        }
    }

    // phaseShift: [0, 1), subdivisions: integer
    private getTracePoints<T>(entryNode: number, phaseShift: number, dots: number, callback: (phaseShift: number, subdivisions: number) => T[]): T[] {
        let list: T[] = [];

        let dist: number[];
        if (this.distCache[entryNode].length === 0)
            this.distCache[entryNode] = dist = Graph.dijkstra(this.graph, entryNode);
        else
            dist = this.distCache[entryNode];

        for (let i = 0; i < this.graph.length; i++) {
            if (dist[i] >= dots)
                continue;

            for (let [j, len] of this.graph[i]) {
                let points = callback.call(this.curveMatrix[i][j], phaseShift, len);
                len = Math.min(len, dots - dist[i]);
                //console.log(`len: ${len}`);
                list = list.concat(points.slice(0, len));
            }
        }
        //console.log(`dots: ${dots}`);
        //console.log(list);

        return list;
    }

    getPoints(entryNode: number, phaseShift: number, dots: number): Point[] {
        return this.getTracePoints(entryNode, phaseShift, dots, CubicBezier.prototype.getPoints);
    }

    getColors(entryNode: number, phaseShift: number, dots: number): RgbColor[] {
        return this.getTracePoints(entryNode, phaseShift, dots, CubicBezier.prototype.getColors);
    }

    getShadedPoints(entryNode: number, phaseShift: number, dots: number): ShadedPoint[] {
        return this.getTracePoints(entryNode, phaseShift, dots, CubicBezier.prototype.getShadedPoints);
    }
}