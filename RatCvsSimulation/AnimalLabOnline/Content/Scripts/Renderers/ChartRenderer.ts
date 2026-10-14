class ChartRenderer {
    g: SVGGElement;
    canvasSize: Size;

    constructor(canvasSize: Size, gSelector: string);
    constructor(canvasSize: Size, g: SVGGElement);

    constructor(canvasSize: Size, gSelectorOrElem: SVGGElement | string) {
        this.canvasSize = canvasSize;
        if (typeof (gSelectorOrElem) === "string") {
            this.g = d3.select(gSelectorOrElem).node() as SVGGElement;
        }
        else {
            this.g = gSelectorOrElem as SVGGElement;
        }
    }

    draw(indicator: string, unit: string, results: [number, number][]) {
        const margin = new Thickness(40, 24);
        const widthI = 6, colorI = "#000";
        const axisLabelDist = 4;

        const g0 = d3.select(this.g);

        const barBaseColor = Settings.current.chartBarBaseColor.toHtml();
        const barTipColor = Settings.current.chartBarTipColor.toHtml();

        g0.style("background", Settings.current.chartBackColor.toHtml())
            .selectAll("*")
            .remove();

        const data = results.map((o, i) => { return { group: `Group ${i + 1}`, value: o[0], deviation: o[1] }; })

        const x = d3
            .scaleBand()
            .domain(data.map(o => o.group))
            .paddingInner(0.4)
            .paddingOuter(0.6)
            .range([margin.left, this.canvasSize.width - margin.right]);

        const y = d3
            .scaleLinear()
            .domain([0, d3.max(data, o => o.value + o.deviation) as number])
            .nice()
            .range([this.canvasSize.height - margin.bottom, margin.top]);

        // defs
        g0.append("defs")
            .append("linearGradient")
            .attr("id", "grad1")
            .attr("x1", 0.5)
            .attr("x2", 0.5)
            .attr("y1", 1)
            .call(o => o.append("stop").attr("offset", 0).attr("stop-color", barBaseColor))
            .call(o => o.append("stop").attr("offset", 1).attr("stop-color", barTipColor));

        // bars
        g0.append("g")
            .selectAll("g")
            .data(data)
            .enter()
            .filter(o => o.value != 0 && o.deviation != 0)
            .append("g")
            .attr("transform", o => `translate(${x(o.group)},0)`)
            .call(g => g.append("rect")
                .attr("width", x.bandwidth())
                .attr("y", o => y(o.value))
                .attr("height", o => y(0) - y(o.value))
                .attr("fill", "url(#grad1)")
            )
            .call(g => g.append("text")
                .attr("x", x.bandwidth() / 2)
                .attr("y", o => y(o.value - o.deviation) + 10)
                .attr("fill", "#fff")
                .attr("font-size", 10)
                .attr("dominant-baseline", "baseline")
                .attr("text-anchor", "middle")
                .text(o => o.value)
            )
            .call(g => g.append("line")
                .attr("x1", x.bandwidth() / 2)
                .attr("x2", x.bandwidth() / 2)
                .attr("y1", o => y(o.value - o.deviation))
                .attr("y2", o => y(o.value + o.deviation))
                .attr("stroke", colorI)
            )
            .call(g => g.append("line")
                .attr("x1", (x.bandwidth() - widthI) / 2)
                .attr("x2", (x.bandwidth() + widthI) / 2)
                .attr("y1", o => y(o.value - o.deviation))
                .attr("y2", o => y(o.value - o.deviation))
                .attr("stroke", colorI)
            )
            .call(g => g.append("line")
                .attr("x1", (x.bandwidth() - widthI) / 2)
                .attr("x2", (x.bandwidth() + widthI) / 2)
                .attr("y1", o => y(o.value + o.deviation))
                .attr("y2", o => y(o.value + o.deviation))
                .attr("stroke", colorI)
            );


        // x axis
        g0.append<SVGGElement>("g")
            .attr("transform", `translate(0,${y(0)})`)
            .call(d3.axisBottom(x))
            .append("text")
            .attr("x", x.range()[1])
            .attr("y", -axisLabelDist)
            .attr("fill", "#000")
            .attr("text-anchor", "end")
            .attr("font-weight", "bold")
            .text("Group");

        // y axis
        g0.append<SVGGElement>("g")
            .attr("transform", `translate(${margin.left},0)`)
            .call(d3.axisLeft(y))
            .call(o => o.select(".domain").remove())
            .select(".tick:last-of-type text")
            .clone()
            .attr("x", axisLabelDist)
            .attr("text-anchor", "start")
            .attr("font-weight", "bold")
            .text(unit === "" ? indicator : `${indicator} (${unit})`);
    }
}