class RgbaColor extends RgbColor {
    // a range: [0, 1]
    a: number;

    // rgba range: [0, 1]
    constructor(r: number = 0, g: number = 0, b: number = 0, a: number = 1) {
        super(r, g, b);
        this.a = a;
    }

    // hsla components are in range [0, 1]
    static fromHsla(h: number, s: number, l: number, a: number): RgbaColor {
        let rgb = RgbColor.fromHsl(h, s, l)
        return new RgbaColor(rgb.r, rgb.g, rgb.b, a);
    }

    // returned rgba components range: [0, 1]
    static fromHtml(color: string): RgbaColor {
        if (!color.startsWith('#') || color.length === 4 || color.length === 7) {
            let { r, g, b } = RgbColor.fromHtml(color);
            return new RgbaColor(r, g, b);
        }

        let r: string, g: string, b: string, a: string;

        if (color.length === 5) // #ffff
            r = color[1] + color[1], g = color[2] + color[2], b = color[3] + color[3], a = color[4] + color[4];
        else if (color.length === 9) // #ffffffff
            r = color.substr(1, 2), g = color.substr(3, 2), b = color.substr(5, 2), a = color.substr(7, 2);
        else
            return new RgbaColor();

        return new RgbaColor(parseInt(r, 16) / 255, parseInt(g, 16) / 255, parseInt(b, 16) / 255, parseInt(a, 16) / 255);
    }

    toHsla(): HslaColor {
        return HslaColor.fromRgba(this.r, this.g, this.b, this.a);
    }

    toHtml(): string {
        return `rgba(${(this.r * 255) | 0},${(this.g * 255) | 0},${(this.b * 255) | 0},${this.a})`;
    }

    toArray(): number[] {
        return [this.r, this.g, this.b, this.a];
    }

    static lerp(c1: RgbaColor, c2: RgbaColor, position: number) {
        return new RgbaColor(c1.r * (1 - position) + c2.r * position, c1.g * (1 - position) + c2.g * position, c1.b * (1 - position) + c2.b * position, c1.a * (1 - position) + c2.a * position);
    }
}