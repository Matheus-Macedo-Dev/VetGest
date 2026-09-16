namespace VetGest.Web.Services.Today;

public sealed class MockTodayService : ITodayService
{
    private static readonly TodayViewModel DemoToday = new(
        "Amora", "Cadela", "Golden Retriever", 28.4m, 42, 63, 21, new DateOnly(2026, 9, 25),
        "Desenvolvimento e crescimento",
        "Os filhotes seguem crescendo e o corpo da Amora começa a pedir mais pausas.",
        "Os órgãos continuam amadurecendo e os movimentos ficam mais coordenados nesta etapa.",
        "O abdômen pode estar mais evidente e o apetite pode oscilar. Cada animal tem seu próprio ritmo.",
        ["Mantenha água fresca e uma rotina calma.", "Observe apetite, disposição e qualquer mudança fora do habitual.", "Anote dúvidas para levar à próxima conversa veterinária."],
        "Consulta de acompanhamento · 18/09", "Attention",
        [new TodayAlertItem("Apetite reduzido", "Apetite diminuído — mantenha observação e considere contato com o veterinário se persistir.")],
        "Estes alertas são educativos e não substituem a avaliação de um médico-veterinário. Em caso de sinais de Atenção ou Emergência, entre em contato com seu veterinário.",
        "Data de cobertura", new DateOnly(2026, 8, 1), "A data de cobertura oferece uma estimativa; a janela pode variar entre animais.", false);

    public async Task<TodayState> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(180, cancellationToken);
        return TodayState.Demo(DemoToday);
    }
}