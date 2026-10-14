class Settings {
    static current: Settings;

    // simulation section
    gizmoRadius = 1;
    gizmoLineWidth = 1;
    gizmoTextColor = "gray";
    gizmoLineColor = "gray";

    outlineLineWidth = 1;
    outlineTextColor = "gray";
    outlineLineColor = "gray";

    useCircleDots = false;
    traceDotSize = 1;
    dotSize = 2;

    injectCycles = 8;
    injectRounds = 1;

    selectorCircleColor = RgbaColor.fromHtml("#9999").toHtml();
    selectorCircleColorHover = RgbaColor.fromHtml("#6669").toHtml();
    selectorCircleColorActive = RgbaColor.fromHtml("#4449").toHtml();
    selectorTextColor = RgbColor.fromHtml("black").toHtml();

    bloodRedColor = RgbColor.fromHtml("#d9392d");
    bloodBlueColor = RgbColor.fromHtml("#2683be");
    bloodPurpleColor = RgbColor.fromHtml("#942e53");
    drugColor = RgbColor.fromHtml("goldenrod");

    // results section
    chartBackColor = RgbColor.fromHtml(Settings.getCssVariable("back-secondary"));
    chartBarTipColor = RgbaColor.fromHtml(Settings.getCssVariable("primary"));
    chartBarBaseColor = RgbaColor.fromHtml(Settings.getCssVariable("secondary"));

    static getCssVariable(name: string): string {
        return getComputedStyle(document.documentElement as HTMLElement).getPropertyValue(`--${name}`).trim();
    }
}