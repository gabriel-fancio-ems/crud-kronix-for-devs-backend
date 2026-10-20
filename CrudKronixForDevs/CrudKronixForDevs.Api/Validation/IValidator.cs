namespace CrudKronixForDevs.Api.Validation
{
    /// <summary>
    /// Contrato simples de validação de entrada.
    /// Retorna um dicionário "campo → lista de erros". Dicionário vazio = tudo certo.
    ///
    /// Em projetos maiores é comum usar a biblioteca FluentValidation para isso;
    /// aqui fazemos à mão para não esconder nenhuma "mágica".
    /// </summary>
    public interface IValidator<in T>
    {
        IDictionary<string, string[]> Validate(T instance);
    }
}
