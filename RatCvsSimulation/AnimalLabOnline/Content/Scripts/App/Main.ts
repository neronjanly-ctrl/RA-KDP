function jsmeOnLoad() {
    AnimalLabApp.current.onMolEditorLoad();
}

function setupAnimation(names: string[], nodes: number[][], edges: number[][], selectors: [string, number, number][], therapyDrug: DrugProtocol, inducerDrug: DrugProtocol) {
    AnimalLabApp.current.animationManager = new AnimationManager(names, nodes, edges, selectors, therapyDrug, inducerDrug);
}

function setupIndexPage() {
    Settings.current = new Settings();

    let app = AnimalLabApp.current = new AnimalLabApp();
    app.setupNavigation();
    app.start();
}

$("#cookieConsent button[data-cookie-string]").click(el => {
    if (el.target.dataset.cookieString !== undefined)
        document.cookie = el.target.dataset.cookieString;
});
