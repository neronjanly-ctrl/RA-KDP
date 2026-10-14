class RgbColor {
    // rgb range: [0, 1]
    r: number;
    g: number;
    b: number;

    // rgb range: [0, 1]
    constructor(r: number = 0, g: number = 0, b: number = 0) {
        this.r = r;
        this.g = g;
        this.b = b;
    }

    private static hueToRgb(p: number, q: number, t: number): number {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1 / 6) return p + (q - p) * 6 * t;
        if (t < 1 / 2) return q;
        if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
        return p;
    }

    // hsl components are in range [0, 1]
    static fromHsl(h: number, s: number, l: number): RgbColor {
        let [r, g, b] = [l, l, l]; // achromatic

        if (s !== 0) {
            const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            const p = 2 * l - q;
            r = RgbColor.hueToRgb(p, q, h + 1 / 3);
            g = RgbColor.hueToRgb(p, q, h);
            b = RgbColor.hueToRgb(p, q, h - 1 / 3);
        }

        return new RgbColor(r, g, b);
    }

    static colorNames = new Map<string, string>([
        ["aliceblue", "#f0f8ff"], // AliceBlue
        ["antiquewhite", "#faebd7"], // AntiqueWhite
        ["aqua", "#00ffff"], // Aqua
        ["aquamarine", "#7fffd4"], // Aquamarine
        ["azure", "#f0ffff"], // Azure
        ["beige", "#f5f5dc"], // Beige
        ["bisque", "#ffe4c4"], // Bisque
        ["black", "#000000"], // Black
        ["blanchedalmond", "#ffebcd"], // BlanchedAlmond
        ["blue", "#0000ff"], // Blue
        ["blueviolet", "#8a2be2"], // BlueViolet
        ["brown", "#a52a2a"], // Brown
        ["burlywood", "#deb887"], // BurlyWood
        ["cadetblue", "#5f9ea0"], // CadetBlue
        ["chartreuse", "#7fff00"], // Chartreuse
        ["chocolate", "#d2691e"], // Chocolate
        ["coral", "#ff7f50"], // Coral
        ["cornflowerblue", "#6495ed"], // CornflowerBlue
        ["cornsilk", "#fff8dc"], // Cornsilk
        ["crimson", "#dc143c"], // Crimson
        ["cyan", "#00ffff"], // Cyan
        ["darkblue", "#00008b"], // DarkBlue
        ["darkcyan", "#008b8b"], // DarkCyan
        ["darkgoldenrod", "#b8860b"], // DarkGoldenRod
        ["darkgray", "#a9a9a9"], // DarkGray
        ["darkgrey", "#a9a9a9"], // DarkGrey
        ["darkgreen", "#006400"], // DarkGreen
        ["darkkhaki", "#bdb76b"], // DarkKhaki
        ["darkmagenta", "#8b008b"], // DarkMagenta
        ["darkolivegreen", "#556b2f"], // DarkOliveGreen
        ["darkorange", "#ff8c00"], // DarkOrange
        ["darkorchid", "#9932cc"], // DarkOrchid
        ["darkred", "#8b0000"], // DarkRed
        ["darksalmon", "#e9967a"], // DarkSalmon
        ["darkseagreen", "#8fbc8f"], // DarkSeaGreen
        ["darkslateblue", "#483d8b"], // DarkSlateBlue
        ["darkslategray", "#2f4f4f"], // DarkSlateGray
        ["darkslategrey", "#2f4f4f"], // DarkSlateGrey
        ["darkturquoise", "#00ced1"], // DarkTurquoise
        ["darkviolet", "#9400d3"], // DarkViolet
        ["deeppink", "#ff1493"], // DeepPink
        ["deepskyblue", "#00bfff"], // DeepSkyBlue
        ["dimgray", "#696969"], // DimGray
        ["dimgrey", "#696969"], // DimGrey
        ["dodgerblue", "#1e90ff"], // DodgerBlue
        ["firebrick", "#b22222"], // FireBrick
        ["floralwhite", "#fffaf0"], // FloralWhite
        ["forestgreen", "#228b22"], // ForestGreen
        ["fuchsia", "#ff00ff"], // Fuchsia
        ["gainsboro", "#dcdcdc"], // Gainsboro
        ["ghostwhite", "#f8f8ff"], // GhostWhite
        ["gold", "#ffd700"], // Gold
        ["goldenrod", "#daa520"], // GoldenRod
        ["gray", "#808080"], // Gray
        ["grey", "#808080"], // Grey
        ["green", "#008000"], // Green
        ["greenyellow", "#adff2f"], // GreenYellow
        ["honeydew", "#f0fff0"], // HoneyDew
        ["hotpink", "#ff69b4"], // HotPink
        ["indianred", "#cd5c5c"], // IndianRed
        ["indigo", "#4b0082"], // Indigo
        ["ivory", "#fffff0"], // Ivory
        ["khaki", "#f0e68c"], // Khaki
        ["lavender", "#e6e6fa"], // Lavender
        ["lavenderblush", "#fff0f5"], // LavenderBlush
        ["lawngreen", "#7cfc00"], // LawnGreen
        ["lemonchiffon", "#fffacd"], // LemonChiffon
        ["lightblue", "#add8e6"], // LightBlue
        ["lightcoral", "#f08080"], // LightCoral
        ["lightcyan", "#e0ffff"], // LightCyan
        ["lightgoldenrodyellow", "#fafad2"], // LightGoldenRodYellow
        ["lightgray", "#d3d3d3"], // LightGray
        ["lightgrey", "#d3d3d3"], // LightGrey
        ["lightgreen", "#90ee90"], // LightGreen
        ["lightpink", "#ffb6c1"], // LightPink
        ["lightsalmon", "#ffa07a"], // LightSalmon
        ["lightseagreen", "#20b2aa"], // LightSeaGreen
        ["lightskyblue", "#87cefa"], // LightSkyBlue
        ["lightslategray", "#778899"], // LightSlateGray
        ["lightslategrey", "#778899"], // LightSlateGrey
        ["lightsteelblue", "#b0c4de"], // LightSteelBlue
        ["lightyellow", "#ffffe0"], // LightYellow
        ["lime", "#00ff00"], // Lime
        ["limegreen", "#32cd32"], // LimeGreen
        ["linen", "#faf0e6"], // Linen
        ["magenta", "#ff00ff"], // Magenta
        ["maroon", "#800000"], // Maroon
        ["mediumaquamarine", "#66cdaa"], // MediumAquaMarine
        ["mediumblue", "#0000cd"], // MediumBlue
        ["mediumorchid", "#ba55d3"], // MediumOrchid
        ["mediumpurple", "#9370db"], // MediumPurple
        ["mediumseagreen", "#3cb371"], // MediumSeaGreen
        ["mediumslateblue", "#7b68ee"], // MediumSlateBlue
        ["mediumspringgreen", "#00fa9a"], // MediumSpringGreen
        ["mediumturquoise", "#48d1cc"], // MediumTurquoise
        ["mediumvioletred", "#c71585"], // MediumVioletRed
        ["midnightblue", "#191970"], // MidnightBlue
        ["mintcream", "#f5fffa"], // MintCream
        ["mistyrose", "#ffe4e1"], // MistyRose
        ["moccasin", "#ffe4b5"], // Moccasin
        ["navajowhite", "#ffdead"], // NavajoWhite
        ["navy", "#000080"], // Navy
        ["oldlace", "#fdf5e6"], // OldLace
        ["olive", "#808000"], // Olive
        ["olivedrab", "#6b8e23"], // OliveDrab
        ["orange", "#ffa500"], // Orange
        ["orangered", "#ff4500"], // OrangeRed
        ["orchid", "#da70d6"], // Orchid
        ["palegoldenrod", "#eee8aa"], // PaleGoldenRod
        ["palegreen", "#98fb98"], // PaleGreen
        ["paleturquoise", "#afeeee"], // PaleTurquoise
        ["palevioletred", "#db7093"], // PaleVioletRed
        ["papayawhip", "#ffefd5"], // PapayaWhip
        ["peachpuff", "#ffdab9"], // PeachPuff
        ["peru", "#cd853f"], // Peru
        ["pink", "#ffc0cb"], // Pink
        ["plum", "#dda0dd"], // Plum
        ["powderblue", "#b0e0e6"], // PowderBlue
        ["purple", "#800080"], // Purple
        ["rebeccapurple", "#663399"], // RebeccaPurple
        ["red", "#ff0000"], // Red
        ["rosybrown", "#bc8f8f"], // RosyBrown
        ["royalblue", "#4169e1"], // RoyalBlue
        ["saddlebrown", "#8b4513"], // SaddleBrown
        ["salmon", "#fa8072"], // Salmon
        ["sandybrown", "#f4a460"], // SandyBrown
        ["seagreen", "#2e8b57"], // SeaGreen
        ["seashell", "#fff5ee"], // SeaShell
        ["sienna", "#a0522d"], // Sienna
        ["silver", "#c0c0c0"], // Silver
        ["skyblue", "#87ceeb"], // SkyBlue
        ["slateblue", "#6a5acd"], // SlateBlue
        ["slategray", "#708090"], // SlateGray
        ["slategrey", "#708090"], // SlateGrey
        ["snow", "#fffafa"], // Snow
        ["springgreen", "#00ff7f"], // SpringGreen
        ["steelblue", "#4682b4"], // SteelBlue
        ["tan", "#d2b48c"], // Tan
        ["teal", "#008080"], // Teal
        ["thistle", "#d8bfd8"], // Thistle
        ["tomato", "#ff6347"], // Tomato
        ["turquoise", "#40e0d0"], // Turquoise
        ["violet", "#ee82ee"], // Violet
        ["wheat", "#f5deb3"], // Wheat
        ["white", "#ffffff"], // White
        ["whitesmoke", "#f5f5f5"], // WhiteSmoke
        ["yellow", "#ffff00"], // Yellow
        ["yellowgreen", "#9acd32"], // YellowGreen
    ]);

    // returned rgb components range: [0, 1]
    static fromHtml(color: string): RgbColor {
        if (!color.startsWith("#")) {
            let color2 = RgbColor.colorNames.get(color.toLowerCase());
            if (color2 === undefined)
                return new RgbColor();
            color = color2;
        }

        let [r, g, b] = [0, 0, 0];

        if (color.length === 4) // #fff
            r = parseInt(color[1] + color[1], 16), g = parseInt(color[2] + color[2], 16), b = parseInt(color[3] + color[3], 16);
        else if (color.length === 7) // #ffffff
            r = parseInt(color.substr(1, 2), 16), g = parseInt(color.substr(3, 2), 16), b = parseInt(color.substr(5, 2), 16);

        return new RgbColor(r / 255, g / 255, b / 255);
    }

    toHsl(): HslColor {
        return HslColor.fromRgb(this.r, this.g, this.b);
    }

    toHtml(): string {
        return `rgb(${(this.r * 255) | 0},${(this.g * 255) | 0},${(this.b * 255) | 0})`;
    }

    toArray(): number[] {
        return [this.r, this.g, this.b];
    }

    static lerp(c1: RgbColor, c2: RgbColor, position: number) {
        return new RgbColor(c1.r * (1 - position) + c2.r * position, c1.g * (1 - position) + c2.g * position, c1.b * (1 - position) + c2.b * position);
    }
}