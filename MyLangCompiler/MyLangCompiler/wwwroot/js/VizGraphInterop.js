async function renderDotToSvg(dot) {
    const viz = new Viz();

    try {
        const svg = await viz.renderString(dot);
        return svg;
    } catch (e) {
        return `<text>Error rendering graph: ${e.message}</text>`;
    }
}