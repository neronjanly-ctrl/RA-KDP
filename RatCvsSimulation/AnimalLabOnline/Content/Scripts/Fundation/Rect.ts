class Rect {
    left: number;
    top: number;
    width: number;
    height: number;

    constructor();
    constructor(width: number, height: number);
    constructor(width: number, height: number, left: number, top: number);

    constructor(width?: number, height?: number, left?: number, top?: number) {
        this.width = width !== undefined ? width : 0;
        this.height = height !== undefined ? height : 0;
        this.left = left !== undefined ? left : 0;
        this.top = top !== undefined ? top : 0;
    }

    static fromCenterSize(centerX: number, centerY: number, width: number, height: number): Rect {
        return new Rect(width, height, centerX - width / 2, centerY - height / 2);
    }

    static fromViewBox(viewBox: string): Rect {
        let comps = viewBox.split(/\s+/).map(o => parseFloat(o));
        return new Rect(comps[2], comps[3], comps[0], comps[1]);
    }

    get right() {
        return this.left + this.width;
    }

    get bottom() {
        return this.top + this.height;
    }

    get topLeft(): Point {
        return new Point(this.left, this.top);
    }

    get topRight(): Point {
        return new Point(this.right, this.top);
    }

    get bottomLeft(): Point {
        return new Point(this.left, this.bottom);
    }

    get bottomRight(): Point {
        return new Point(this.right, this.bottom);
    }

    get center(): Point {
        return new Point(this.left + this.width / 2, this.top + this.height / 2);
    }

    get size(): Size {
        return new Size(this.width, this.height);
    }

    offset(size: Size): Rect;
    offset(dx: number, dy: number): Rect;

    offset(arg1: Size | number, arg2?: number): Rect {
        if (typeof (arg1) === "number") {
            this.left += arg1;
            this.top += arg2 as number;
        }
        else {
            this.left += arg1.width;
            this.top += arg1.height;
        }
        return this;
    }

    resize(newSize: Size): Rect;
    resize(newWidth: number, newHeight: number): Rect;

    resize(arg1: Size | number, arg2?: number): Rect {
        if (typeof (arg1) === "number") {
            this.width = arg1;
            this.height = arg2 as number;
        }
        else {
            this.width = arg1.width;
            this.height = arg1.height;
        }
        return this;
    }
}