using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;
using System;
using System.IO;
using Microsoft.Bot.Builder;

namespace EchoBot1.Dialogs
{
    public class KnowledgeLearningBaseDialog : ComponentDialog
    {
        private readonly KnowledgeLearningBaseService _knowledgeBase;
        private readonly string _localPath;

        public KnowledgeLearningBaseDialog(KnowledgeLearningBaseService knowledgeBase, string localPath) : base(nameof(KnowledgeLearningBaseDialog))
        {
            _knowledgeBase = knowledgeBase ?? throw new ArgumentNullException(nameof(knowledgeBase));
            _localPath = localPath ?? throw new ArgumentNullException(nameof(localPath));

            var waterfallSteps = new WaterfallStep[]
            {
                AskForKnowledgeDetailsStepAsync,
                ConfirmKnowledgeStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForKnowledgeDetailsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "Por favor insira a nova entrada na base de conhecimento no formato: Entidade: <entidade> grupo: <grupo> = <chave> = <resposta>";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptMessage) }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmKnowledgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is string userInput && !string.IsNullOrWhiteSpace(userInput))
            {
                var separators = new[] { ':', '=' };
                var parts = userInput.Split(separators, 6, StringSplitOptions.TrimEntries);

                if (parts.Length == 6)
                {
                    var entidade = parts[1].ToLower();
                    var grupo = parts[3].ToLower();
                    var key = parts[4].ToLower();
                    var response = parts[5];

                    try
                    {
                        // Verifica se o caminho local é válido
                        string entityPath = Path.Combine(_localPath, entidade);
                        if (!Directory.Exists(entityPath))
                        {
                            Directory.CreateDirectory(entityPath);
                        }

                        await _knowledgeBase.AddOrUpdateResponseAsync(entityPath, entidade, grupo, key, response, cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Entrada adicionada ou atualizada com sucesso."), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Erro ao atualizar a base de conhecimento: {ex.Message}"), cancellationToken);
                        return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
                    }
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Formato incorreto. Por favor, siga o formato solicitado."), cancellationToken);
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
                }

                return await stepContext.NextAsync(null, cancellationToken);
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Concluído."), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
