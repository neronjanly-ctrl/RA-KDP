class Thickness {
    left: number;
    right: number;
    top: number;
    bottom: number;

    /**
     * Constructs a new Thickness object with a uniform value.
     *
     * @param uniform The value to be used for all four sides.
     */
    constructor(uniform: number);
    constructor(horizontal: number, vertical: number);
    constructor(left: number, top: number, right: number, bottom: number);

    constructor(v1?: number, v2?: number, v3?: number, v4?: number) {
        if (v1 === undefined)
            this.left = this.right = this.top = this.bottom = 0;
        else if (v2 === undefined)
            this.left = this.right = this.top = this.bottom = v1;
        else if (v3 === undefined)
            this.left = this.right = v1, this.top = this.bottom = v2;
        else if (v4 === undefined)
            this.left = v1, this.top = v2, this.right = v3, this.bottom = 0;
        else
            this.left = v1, this.top = v2, this.right = v3, this.bottom = v4;
    }
}