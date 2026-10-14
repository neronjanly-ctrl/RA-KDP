class CubicBezierPoint {
    endPoint: Point;
    controlPoint: Point;

    constructor(epx: number, epy: number, cpx: number, cpy: number) {
        this.endPoint = new Point(epx, epy);
        this.controlPoint = new Point(cpx, cpy);
    }
}