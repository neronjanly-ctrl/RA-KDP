class Array2D {
    static create<T>(length0: number, length1: number, value?: T): T[][] {
        if (value === undefined)
            return Array<T[]>(length0).fill(Array<T>(length1));
        return Array<T[]>(length0).fill(Array<T>(length1).fill(value));
    }

    static createSquare<T>(length: number, value?: T): T[][] {
        if (value === undefined)
            return Array<T[]>(length).fill(Array<T>(length));
        return Array<T[]>(length).fill(Array<T>(length).fill(value));
    }
}