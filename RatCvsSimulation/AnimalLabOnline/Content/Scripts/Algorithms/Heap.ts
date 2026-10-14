class Heap<T> {
    data: T[];
    less: (a: T, b: T) => boolean;

    constructor(less?: (a: T, b: T) => boolean, data?: T[]) {
        if (less !== undefined)
            this.less = less;
        else
            this.less = (a, b) => a < b;

        this.data = [];
        if (data !== undefined)
            for (let i of data)
                this.push(i);
    }

    private swap(i: number, j: number) {
        let t = this.data[i];
        this.data[i] = this.data[j];
        this.data[j] = t;
    }

    get length() {
        return this.data.length;
    }

    empty(): boolean {
        return this.length === 0;
    }

    push(item: T) {
        this.data.push(item);

        for (let i = this.data.length - 1, j; i > 0; i = j) {
            j = (i - 1) >> 1; // parent
            let vi = this.data[i], vj = this.data[j]; // values

            if (!this.less(vj, vi))
                break;

            this.swap(i, j);
        }
    }

    top(): T {
        return this.data[0];
    }

    pop() {
        this.swap(0, this.data.length - 1);
        this.data.pop();

        for (let i = 0; ;) {
            let j = (i << 1) + 1, k = (i << 1) + 2;

            if (j >= this.data.length)
                break;

            let vi = this.data[i], vj = this.data[j];

            if (k < this.data.length) {
                let vk = this.data[k];

                if (this.less(vj, vk))
                    j = k, vj = vk;
            }

            if (!this.less(vi, vj))
                break;

            this.swap(i, j), i = j;
        }
    }
}