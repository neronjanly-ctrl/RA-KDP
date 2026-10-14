class HslColor {
    // hsl range: [0, 1]
    h: number;
    s: number;
    l: number;

    // hsl range: [0, 1]
    constructor(h: number = 0, s: number = 0, l: number = 0) {
        this.h = h;
        this.s = s;
        this.l = l;
    }

    // rgb components are in range [0, 1]
    static fromRgb(r: number, g: number, b: number): HslColor {
        const max = Math.max(r, g, b), min = Math.min(r, g, b);

        let [h, s, l] = [0, 0, (max + min) / 2];

        if (max !== min) {
            const df = max - min, sm = max + min;

            s = l > 0.5 ? df / (2 - sm) : df / sm;

            switch (max) {
                case r: h = (g - b) / df + (g < b ? 6 : 0);
                    break;
                case g: h = (b - r) / df + 2;
                    break;
                case b: h = (r - g) / df + 4;
                    break;
            }

            h /= 6;
        }

        return new HslColor(h, s, l);
    }

    // returned hsl components are in range [0, 1]
    static fromHtml(color: string): HslColor {
        const { r, g, b } = RgbColor.fromHtml(color);

        return HslColor.fromRgb(r, g, b);
    }

    toRgb(): RgbColor {
        return RgbColor.fromHsl(this.h, this.s, this.l);
    }

    toHtml(): string {
        return `hsl(${(this.h * 360) | 0},${(this.s * 100) | 0}%,${(this.l * 100) | 0}%)`;
    }

    toArray(): number[] {
        return [this.h, this.s, this.l];
    }

    static lerp(c1: HslColor, c2: HslColor, position: number) {
        return new HslColor(c1.h * (1 - position) + c2.h * position, c1.s * (1 - position) + c2.s * position, c1.l * (1 - position) + c2.l * position);
    }
}