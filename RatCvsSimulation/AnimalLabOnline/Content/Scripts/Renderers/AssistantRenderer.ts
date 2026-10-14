class AssistantRenderer {
    g: SVGGElement;

    constructor(gSelector: string);
    constructor(g: SVGGElement);

    constructor(gSelectorOrElem: SVGGElement | string) {
        if (typeof (gSelectorOrElem) === "string") {
            this.g = d3.select(gSelectorOrElem).node() as SVGGElement;
        }
        else {
            this.g = gSelectorOrElem as SVGGElement;
        }
    }

    draw(color: HslaColor | HslColor | RgbaColor | RgbColor | string) {
        let g = d3.select(this.g);

        let c = typeof (color) !== "string" ? color.toHtml() : color;

        g.append("polygon") // head
            .attr("points", "98,54 102,54 100,58")
            .attr("fill", "none")
            .attr("stroke", c)
            .attr("stroke-width", 26)
            .attr("stroke-linecap", "round")
            .attr("stroke-linejoin", "round");

        g.append("rect") // body
            .attr("x", 85)
            .attr("y", 75)
            .attr("width", 30)
            .attr("height", 51)
            .attr("fill", c);

        let g2 = g.append("g")
            .attr("fill", "none")
            .attr("stroke", c)
            .attr("stroke-width", 10)
            .attr("stroke-linecap", "round")
            .attr("stroke-linejoin", "bevel");

        g2.append("polyline") // contour
            .attr("points", "120,75 115,75 115,140 110,140 110,130 90,130 90,140 85,140 85,75 80,75");

        g2.append("polyline") // left arm
            .attr("points", "120,75 125,100 130,125");

        g2.append("polyline") // right arm
            .attr("class", "right-arm")
            .attr("points", "80,75 70,100 60,75");

        g2.append("line") // stick
            .attr("class", "right-stick")
            .attr("x1", 60)
            .attr("y1", 75)
            .attr("x2", 40)
            .attr("y2", 25)
            .attr("stroke-width", 2)
            .attr("stroke-linecap", "square");
    }

    private upperArmLen = 26;
    private lowerArmLen = 30;
    private stickLen = 50;
    private shoulderPos = new Point(80, 75);

    moveStick(hand: Point) {
        let arm = d3.select(this.g).select(".right-arm");
        let stick = d3.select(this.g).select(".right-stick");

        const dist = Math.sqrt((hand.x - this.shoulderPos.x) * (hand.x - this.shoulderPos.x) + (hand.y - this.shoulderPos.y) * (hand.y - this.shoulderPos.y)); // distance between shoulder and hand
        const a = Math.atan2(this.shoulderPos.y - hand.y, this.shoulderPos.x - hand.x); // angle between shoulder-hand line and horizonal line
        const b = Math.asin((dist * dist + this.upperArmLen * this.upperArmLen - this.lowerArmLen * this.lowerArmLen) / (2 * dist * this.upperArmLen)); // angle between upper-arm line and the line perpendicular to shoulder-hand line
        const c = Math.PI / 2 + a + b; // angle between horizonal line and upper-arm line
        const ebx = this.shoulderPos.x + this.upperArmLen * Math.cos(c), eby = this.shoulderPos.y + this.upperArmLen * Math.sin(c); // elbow coordinate
        const d = Math.atan2(hand.y - eby, hand.x - ebx); // angle between elbow-hand line and horizontal line
        const stx = hand.x + this.stickLen * Math.cos(d), sty = hand.y + this.stickLen * Math.sin(d); // stick end coordinate

        arm.attr("points", `${this.shoulderPos.x},${this.shoulderPos.y} ${ebx},${eby} ${hand.x},${hand.y}`);
        stick.attr("x1", hand.x);
        stick.attr("y1", hand.y);
        stick.attr("x2", stx);
        stick.attr("y2", sty);
    }
}