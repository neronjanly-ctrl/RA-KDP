class Logger {
    container: JQuery<HTMLElement>;

    constructor(container: JQuery<HTMLElement>) {
        this.container = container;
    }

    private log(msg: string) {
        this.container.append(`<div>${msg}</div>`);
    }

    info(msg: string) {
        this.log(`[info] ${msg}`);
    }

    warn(msg: string) {
        this.log(`[warn] ${msg}`);
    }

    error(msg: string) {
        this.log(`[error] ${msg}`);
    }
}