function camposCondicionales() {
    for (const select of document.querySelectorAll("[data-controla]")) {
        const refrescar = () => {
            for (const grupo of document.querySelectorAll("[data-grupo='" + select.dataset.controla + "']")) {
                const mostrar = grupo.dataset.valor.split(",").includes(select.value);
                grupo.hidden = !mostrar;
                for (const campo of grupo.querySelectorAll("input, select, textarea")) campo.disabled = !mostrar;
            }
        };
        select.addEventListener("change", refrescar);
        refrescar();
    }
}
camposCondicionales();
