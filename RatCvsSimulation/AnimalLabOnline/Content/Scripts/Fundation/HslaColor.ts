class HslaColor extends HslColor {
    // a range: [0, 1]
    a: number;

    // hsla range: [0, 1]
    constructor(h: number = 0, s: number = 0, l: number = 0, a: number = 1) {
        super(h, s, l);
        this.a = a;
    }

    // rgba components are in range [0, 1]
    static fromRgba(r: number, g: number, b: number, a: number): HslaColor {
        let hsl = HslColor.fromRgb(r, g, b);
        return new HslaColor(hsl.h, hsl.s, hsl.l, a);
    }

    // returned hsla components are in range [0, 1]
    static fromHtml(color: string): HslaColor {
        const { r, g, b, a } = RgbaColor.fromHtml(color);
        return HslaColor.fromRgba(r, g, b, a);
    }

    toRgba(): RgbaColor {
        return RgbaColor.fromHsla(this.h, this.s, this.l, this.a);
    }

    toHtml(): string {
        return `hsla(${(this.h * 360) | 0},${(this.s * 100) | 0}%,${(this.l * 100) | 0}%,${this.a})`;
    }

    toArray(): number[] {
        return [this.h, this.s, this.l, this.a];
    }

    static lerp(c1: HslaColor, c2: HslaColor, position: number) {
        return new HslaColor(c1.h * (1 - position) + c2.h * position, c1.s * (1 - position) + c2.s * position, c1.l * (1 - position) + c2.l * position, c1.a * (1 - position) + c2.a * position);
    }
}