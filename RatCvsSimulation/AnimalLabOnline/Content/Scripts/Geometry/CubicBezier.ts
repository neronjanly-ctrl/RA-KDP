class CubicBezier {
    readonly points: CubicBezierPoint[];
    readonly weights: number[];
    readonly colors: RgbColor[];
    readonly totalWeight: number;
    readonly closed: boolean;

    // [][0 end_point.x, 1 end_point.y, 2 control_point.x, 3 control_point.y, 4 curve_length, 5 color.r, 6 color.g, 7 color.b]
    constructor(pointArr: number[][], closed: boolean) {
        this.points = [];
        this.weights = [];
        this.colors = [];
        this.totalWeight = 0;
        this.closed = closed;

        for (let pts of pointArr) {
            this.points.push(new CubicBezierPoint(pts[0], pts[1], pts[2], pts[3]));
            this.weights.push(pts[4]);
            this.colors.push(new RgbColor(pts[5], pts[6], pts[7]));
            this.totalWeight += pts[4];
        }
        if (!closed && pointArr.length > 0)
            this.totalWeight -= pointArr[pointArr.length - 1][4];
        //console.log(pointArr);
    }

    private getPointAt(index: number, position: number): Point {
        const p0 = this.points[index], p1 = this.points[(index + 1) % this.points.length];
        const cp0 = Point.add(p0.endPoint, Point.minus(p0.endPoint, p0.controlPoint)), cp1 = p1.controlPoint;
        const mp0 = Point.lerp(p0.endPoint, cp0, position), mp1 = Point.lerp(cp0, cp1, position), mp2 = Point.lerp(cp1, p1.endPoint, position);
        const np0 = Point.lerp(mp0, mp1, position), np1 = Point.lerp(mp1, mp2, position);
        return Point.lerp(np0, np1, position);
    }

    private getColorAt(index: number, position: number): RgbColor {
        return RgbColor.lerp(this.colors[index], this.colors[(index + 1) % this.colors.length], position);
    }

    private getShadedPointAt(index: number, position: number): ShadedPoint {
        return new ShadedPoint(this.getPointAt(index, position), this.getColorAt(index, position));
    }

    private iteratePoints<T>(position: number, callback: (index: number, position: number) => T): T {
        let tw = position * this.totalWeight;

        for (let i = 0; i < this.points.length; i++) {
            const w = this.weights[i];
            if (tw - w <= 0) {
                return callback.call(this, i, tw / w);
            }
            tw -= w;
        }

        return callback.call(this, this.points.length - 1, this.closed ? 1 : 0);
    }

    getPoint(position: number): Point {
        return this.iteratePoints(position, this.getPointAt);
    }

    getColor(position: number): RgbColor {
        return this.iteratePoints(position, this.getColorAt);
    }

    getShadedPoint(position: number): ShadedPoint {
        return this.iteratePoints(position, this.getShadedPointAt);
    }

    // phaseShift: [0, 1), subdivisions: integer
    private getAllPoints<T>(phaseShift: number, subdivisions: number, callback: (index: number, position: number) => T): T[] {
        let list: T[] = [];

        let p = 0, tw0 = phaseShift / subdivisions * this.totalWeight, acctw = 0;
        for (let i = 0; i < subdivisions && p < this.weights.length; i++) {
            const tw = tw0 + i / subdivisions * this.totalWeight - acctw;
            const w = this.weights[p];

            if (tw - w <= 0) {
                list.push(callback.call(this, p, tw / w));
            }
            else {
                p++;
                i--;
                acctw += w;
            }
        }

        return list;
    }

    getPoints(phaseShift: number, subdivisions: number): Point[] {
        return this.getAllPoints(phaseShift, subdivisions, this.getPointAt);
    }

    getColors(phaseShift: number, subdivisions: number): RgbColor[] {
        return this.getAllPoints(phaseShift, subdivisions, this.getColorAt);
    }

    getShadedPoints(phaseShift: number, subdivisions: number): ShadedPoint[] {
        return this.getAllPoints(phaseShift, subdivisions, this.getShadedPointAt);
    }

    drawGizmos(ctx: CanvasRenderingContext2D) {
        ctx.save();
        ctx.fillStyle = Settings.current.gizmoTextColor;
        ctx.strokeStyle = Settings.current.gizmoLineColor;
        ctx.lineWidth = Settings.current.gizmoLineWidth;

        let i = 0;
        for (let pt of this.points) {
            const cp = pt.controlPoint, ep = pt.endPoint;
            ctx.beginPath();
            ctx.arc(cp.x, cp.y, Settings.current.gizmoRadius, 0, Math.PI * 2);
            ctx.closePath();
            ctx.stroke();

            ctx.beginPath();
            ctx.arc(ep.x, ep.y, Settings.current.gizmoRadius, 0, Math.PI * 2);
            ctx.closePath();
            ctx.stroke();

            ctx.beginPath();
            ctx.moveTo(cp.x, cp.y);
            ctx.lineTo(ep.x, ep.y);
            ctx.closePath();
            ctx.stroke();

            ctx.textAlign = "center";
            ctx.textBaseline = "top";
            ctx.fillText(`[${i++}] ${ep.x},${ep.y}`, cp.x, cp.y);
        }
        ctx.restore();
    }

    drawPath(ctx: CanvasRenderingContext2D) {
        ctx.beginPath();
        ctx.moveTo(this.points[0].endPoint.x, this.points[0].endPoint.y);

        for (let i = 1; i <= this.points.length; i++) {
            const cp2 = this.points[i % this.points.length].controlPoint, ep2 = this.points[i % this.points.length].endPoint;
            const cp1 = this.points[i - 1].controlPoint, ep1 = this.points[i - 1].endPoint;
            const rcp1 = Point.add(ep1, Point.minus(ep1, cp1));
            ctx.bezierCurveTo(rcp1.x, rcp1.y, cp2.x, cp2.y, ep2.x, ep2.y);
        }

        ctx.closePath();
    }

    fill(ctx: CanvasRenderingContext2D) {
        this.drawPath(ctx);
        ctx.fill();
    }

    stroke(ctx: CanvasRenderingContext2D) {
        this.drawPath(ctx);
        ctx.stroke();
    }
}
