using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class IssueResolutionDialog : ComponentDialog
{
    public IssueResolutionDialog() : base(nameof(IssueResolutionDialog))
    {
        var waterfallSteps = new WaterfallStep[]
        {
            FixIssuesStepAsync,
            CloseProgramsStepAsync,
            FinalStepAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));
    }

    // Etapa 1: Resolver Problemas
    private async Task<DialogTurnResult> FixIssuesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Estamos começando a resolver os problemas que você encontrou."), cancellationToken);
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        {
            Prompt = MessageFactory.Text("Por favor, descreva o problema que você está enfrentando.")
        }, cancellationToken);
    }

    // Etapa 2: Fechar Programas
    private async Task<DialogTurnResult> CloseProgramsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var problemDescription = (string)stepContext.Result;

        // Adicione a lógica para identificar os programas a serem fechados com base na descrição do problema
        // Neste exemplo, fecharemos programas com base em nomes específicos
        var programsToClose = new[] { "notepad", "chrome" }; // Lista de programas para fechar

        foreach (var program in programsToClose)
        {
            await CloseProgramAsync(program);
        }

        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Programas relacionados ao problema foram fechados."), cancellationToken);

        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        {
            Prompt = MessageFactory.Text("Você gostaria de adicionar mais algum detalhe ou ação? Se não, digite 'não'.")
        }, cancellationToken);
    }

    // Método para fechar um programa específico
    private async Task CloseProgramAsync(string processName)
    {
        // Encerra o processo pelo nome
        var processes = Process.GetProcessesByName(processName);
        foreach (var process in processes)
        {
            try
            {
                process.Kill();
                await Task.Delay(100); // Aguarda um pouco para garantir que o processo tenha tempo para ser fechado
            }
            catch
            {
                // Lida com exceções, se necessário
            }
        }
    }

    // Etapa 3: Finalizar o Diálogo
    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var userResponse = (string)stepContext.Result;

        if (userResponse.ToLower().Contains("não"))
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ótimo! Se precisar de mais ajuda, é só chamar."), cancellationToken);
        }
        else
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Adicionando detalhes ou ações adicionais..."), cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(EchoBot1), null, cancellationToken);
        }
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
