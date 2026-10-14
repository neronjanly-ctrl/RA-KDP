interface JQuery {
    gatedClick(handler?: JQuery.EventHandlerBase<void, JQuery.ClickEvent<HTMLElement, null, HTMLElement, HTMLElement>> | false): this;
}

$.fn.gatedClick = function (handler?: JQuery.EventHandlerBase<void, JQuery.ClickEvent<HTMLElement, null, HTMLElement, HTMLElement>>) {
    this.click(event => {
        if (handler !== undefined && !$(event.currentTarget).hasClass("disabled"))
            return handler(event);
        return false;
    });
    return this;
}