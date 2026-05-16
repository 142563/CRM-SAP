namespace CRM_ERP_UMG.Services;

public class ServicioFormulas
{
    public decimal Evaluar(string expresion, Dictionary<string, decimal> variables)
    {
        var tokens = Tokenizar(expresion, variables);
        var polacaInversa = ConvertirAPolacaInversa(tokens);
        return EvaluarPolacaInversa(polacaInversa);
    }

    private List<string> Tokenizar(string expresion, Dictionary<string, decimal> variables)
    {
        var tokens = new List<string>();
        int i = 0;
        while (i < expresion.Length)
        {
            char c = expresion[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (char.IsDigit(c) || c == '.')
            {
                int inicio = i;
                while (i < expresion.Length && (char.IsDigit(expresion[i]) || expresion[i] == '.'))
                    i++;
                tokens.Add(expresion[inicio..i]);
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                int inicio = i;
                while (i < expresion.Length && (char.IsLetterOrDigit(expresion[i]) || expresion[i] == '_'))
                    i++;
                string nombre = expresion[inicio..i];
                if (variables.TryGetValue(nombre, out decimal val))
                    tokens.Add(val.ToString(System.Globalization.CultureInfo.InvariantCulture));
                else
                    tokens.Add("0");
                continue;
            }

            tokens.Add(c.ToString());
            i++;
        }
        return tokens;
    }

    private List<string> ConvertirAPolacaInversa(List<string> tokens)
    {
        var salida = new List<string>();
        var operadores = new Stack<string>();
        var precedencia = new Dictionary<string, int> { ["+"] = 1, ["-"] = 1, ["*"] = 2, ["/"] = 2 };

        foreach (var token in tokens)
        {
            if (decimal.TryParse(token, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _))
            {
                salida.Add(token);
            }
            else if (token == "(")
            {
                operadores.Push(token);
            }
            else if (token == ")")
            {
                while (operadores.Count > 0 && operadores.Peek() != "(")
                    salida.Add(operadores.Pop());
                if (operadores.Count > 0) operadores.Pop();
            }
            else if (precedencia.ContainsKey(token))
            {
                while (operadores.Count > 0 && precedencia.ContainsKey(operadores.Peek()) &&
                       precedencia[operadores.Peek()] >= precedencia[token])
                    salida.Add(operadores.Pop());
                operadores.Push(token);
            }
        }
        while (operadores.Count > 0)
            salida.Add(operadores.Pop());
        return salida;
    }

    private decimal EvaluarPolacaInversa(List<string> tokens)
    {
        var pila = new Stack<decimal>();
        foreach (var token in tokens)
        {
            if (decimal.TryParse(token, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal num))
            {
                pila.Push(num);
            }
            else
            {
                if (pila.Count < 2) continue;
                decimal b = pila.Pop();
                decimal a = pila.Pop();
                pila.Push(token switch
                {
                    "+" => a + b,
                    "-" => a - b,
                    "*" => a * b,
                    "/" => b != 0 ? a / b : 0,
                    _ => 0
                });
            }
        }
        return pila.Count > 0 ? pila.Pop() : 0;
    }
}
