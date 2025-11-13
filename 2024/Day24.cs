namespace AoC._2024;

public class Day24 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 24;

    enum Operation
    {
        Or,
        And,
        Xor
    }

    record Rule(string a, Operation op, string b, string res)
    {
        public bool Handled;
    }
    
    protected override object Part1Impl()
    {
        var inputText = Input.Text.AsSpan();
        var input = Input.Text.AsSpan().Split('\n');
        
        Dictionary<string, byte> registers = new Dictionary<string, byte>();
        

        while (input.MoveNext())
        {
            var l = inputText[input.Current];
            if (l.IsEmpty)
                break;

            registers[l[..3].ToString()] = Util.FastParseInt<byte>(l[5..]);
        }

        List<Rule> rules = [];
        
        while (input.MoveNext())
        {
            var l = inputText[input.Current];

            var regA = l[..3];
            var opSpan = l[4..7];
            var nextI = 8;
            Operation op;

            switch (opSpan[0])
            {
                case 'O':
                    nextI--;
                    op = Operation.Or;
                    break;
                case 'X':
                    op = Operation.Xor;
                    break;
                default:
                    op = Operation.And;
                    break;
            }
            
            var regB = l[nextI..(nextI + 3)];
            var outReg = l[(nextI+7)..(nextI + 10)];
            
            rules.Add(new (regA.ToString(), op, regB.ToString(), outReg.ToString()));
        }

        while (true)
        {
            bool anyApplied = false;

            foreach (var rule in rules)
            {
                if (rule.Handled)
                    continue;
                var (a, op, b, res) = rule;

                if (registers.TryGetValue(a, out var aVal)
                    && registers.TryGetValue(b, out var bVal))
                {
                    rule.Handled = true;
                    anyApplied = true;
                    registers[res] = op switch
                    {
                        Operation.Or => (byte)(aVal | bVal),
                        Operation.And => (byte)(aVal & bVal),
                        Operation.Xor => (byte)(aVal ^ bVal),
                    };
                }
            }

            if (!anyApplied)
                break;
        }


        long result = 0;
        foreach (var (name, value) in registers)
        {
            if (value > 0 && name is ['z', _, _])
            {
                var number = Util.FastParse2DigitInt<int>(name.AsSpan(1));
                
                result |= 1L << number;
            }
        }
        
        return result;
    }

    protected override object Part2Impl()
    {
        return -1;
    }
}