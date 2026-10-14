
class Point {
    x: number;
    y: number;

    constructor();
    constructor(x: number, y: number);

    constructor(x?: number, y?: number) {
        this.x = x === undefined ? 0 : x;
        this.y = y === undefined ? 0 : y;
    }

    offset(size: Size): void;
    offset(dx: number, dy: number): void;

    offset(arg1: any, arg2?: number) {
        if (arg2 === undefined) {
            this.x += arg1.width;
            this.y += arg1.height;
        }
        else {
            this.x += arg1;
            this.y += arg2;
        }
    }

    distance(point: Point): number {
        return Math.sqrt((this.x - point.x) * (this.x - point.x) + (this.y - point.y) * (this.y - point.y));
    }

    angle(point: Point): number {
        return Math.atan2(point.y - this.y, point.x - this.x);
    }

    clone(): Point {
        return new Point(this.x, this.y);
    }

    static add(point: Point, size: Size): Point {
        return new Point(point.x + size.width, point.y + size.height);
    }

    static minus(p1: Point, p2: Point): Size {
        return new Size(p1.x - p2.x, p1.y - p2.y);
    }

    static lerp(p1: Point, p2: Point, position: number): Point {
        return new Point(p1.x * (1 - position) + p2.x * position, p1.y * (1 - position) + p2.y * position);
    }
}
