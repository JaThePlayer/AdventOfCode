using Microsoft.Z3;

namespace AoC._2025;

/*
Initial
| Method | Mean       | Error     | StdDev    | Gen0     | Gen1     | Allocated |
|------- |-----------:|----------:|----------:|---------:|---------:|----------:|
| Part1  |   7.587 ms | 0.0528 ms | 0.0413 ms | 875.0000 | 429.6875 |   7.02 MB |
| Part2  | 765.188 ms | 8.1363 ms | 6.3523 ms |        - |        - |   1.96 MB |
 */
public class Day10 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 10;

    record struct Machine(byte[] Target, List<List<int>> Buttons, List<int> Joltage);
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        List<Machine> machines = [];
        foreach (var range in input.Split((byte)'\n'))
        {
            //[.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
            var line = input[range];
            var p = new SpanParserU8(line);
            p.Skip(1);
            var target = p.ReadStrUntil(']');
            p.TrimStart();
            var buttons = new SpanParserU8(p.ReadStrUntil('{'));
            List<List<int>> buttonList = [];
            while (buttons.SliceUntil((byte)')').TryUnpack(out var buttonParser))
            {
                buttonParser.TrimStart();
                buttonParser.TrimEnd();
                if (buttonParser.IsEmpty)
                    continue;
                buttonParser.Skip(1); // (
                var buttonActivated = buttonParser.ParseList<int>(',');
                buttonList.Add(buttonActivated);
            }
            
            var joltages = p.ReadStrUntil('}');
            
            machines.Add(new(target.ToArray(), buttonList, []));
        }

        long sum = 0;
        foreach (var m in machines)
        {
            var state = new byte[m.Target.Length];
            state.AsSpan().Fill((byte)'.');

            sum += SolveP1For(m, state, m.Buttons);
        }

        return sum; // 444
    }

    private byte[] ApplyButton(byte[] state, List<int> button)
    {
        state = (byte[])state.Clone();
        
        foreach (var i in button)
        {
            state[i] = state[i] == (byte)'.' ? (byte)'#' : (byte)'.';
        }

        return state;
    }

    private int SolveP1For(Machine m, byte[] state, List<List<int>> buttons)
    {
        if (state.SequenceEqual(m.Target))
            return 0;

        switch (buttons)
        {
            case []:
                return int.MaxValue;
            case [var only]:
            {
                var newState = ApplyButton(state, only);
                if (newState.SequenceEqual(m.Target))
                    return 1;
                return int.MaxValue;
            }
            case [var first, .. var rest]:
            {
                var newState = ApplyButton(state, first);
                if (newState.SequenceEqual(m.Target))
                    return 1;
                var afterFirstPressed = SolveP1For(m, newState, rest);
                var withoutFirstPressed = SolveP1For(m, state, rest);

                return int.Min(afterFirstPressed == int.MaxValue ? int.MaxValue : afterFirstPressed + 1, withoutFirstPressed);
            }
        }
        
        return int.MaxValue;
    }
    

    protected override object Part2Impl()
    {
        var input = Input.TextU8;
        List<Machine> machines = [];
        foreach (var range in input.Split((byte)'\n'))
        {
            //[.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
            var line = input[range];
            var p = new SpanParserU8(line);
            p.Skip(1);
            var target = p.ReadStrUntil(']');
            p.TrimStart();
            var buttons = new SpanParserU8(p.ReadStrUntil('{'));
            List<List<int>> buttonList = [];
            while (buttons.SliceUntil((byte)')').TryUnpack(out var buttonParser))
            {
                buttonParser.TrimStart();
                buttonParser.TrimEnd();
                if (buttonParser.IsEmpty)
                    continue;
                buttonParser.Skip(1); // (
                var buttonActivated = buttonParser.ParseList<int>(',');
                buttonList.Add(buttonActivated);
            }
            
            var joltages = new SpanParserU8(p.ReadStrUntil('}'));
            
            machines.Add(new(target.ToArray(), buttonList, joltages.ParseList<int>(',')));
        }

        long sum = 0;
        foreach (var m in machines)
        {
            // Adapted from https://topaz.github.io/paste/#XQAAAQBDDgAAAAAAAAAzHIoib6pENkSmUIKIED8dy140D1lKWSKhzWZruQROc6UAuJBRVlu1Mq72DZtF5qzClZMn2WyZP1r61XE2piRe/0aHXN484GT6bQN+eq0DDWw6AIE/9xCs7pv3XTqS0ebIqwdQUfTZ9vX2eAfyCIjLNPnyUdutd2Sw2qji3foIuhF/dPHW0JresHceKrnXbDgcsdshH5fsYqnIa0x6VQ3filPm2b1MHOs3oksM2XLukj31QFHtxeDQbBrLXwyaWlHfWeOUCaAHWrY6BUHlXIFNx/aSgYZWm2O3ap3YP5dmcYsPA4dkPrfWtVEDS5UfvIPfg9SO3PZec8Q4IppVvadfY9Jc3Lv+3q5QhVOsp/mw9Iw8rQcA/Cv7GaKW+ZQyon3A6UJhTXJz94yEx8xe6HMOvbRY4fPzxyThIn2Dl1MrqBF0nhaiqwsM+861l6ZIXTupI4D5+5CI5sBDt2XkXcVzNwi9YEaUOG+b/7NyfsfvmvFEhqDylNatFJVg+SoHqutTgli2F21WR5OuYWzMgSp7j0FC6cVMUiDbvP4CgwtIiO9O8wmivPR3kfvxIJq8nTBAe7MvHYWWDWMdqoHFQqtUVYo0DvROX8sQuey8bfjoXhB8oV008TKpZqOZfe6bMaq/2M/jt5mPerSHbqwuDmnSoUAeC17RqnDc13tRvswmRy2q1BCSNXRQ2MBUlu4FCGoZOWCSwNmM4+KQnuMQ7GrIc//iwJM4KAgRIOzYQDHa+PYJ3SwPmn64dmMHlOL0F9Ko5Xc0tyRcPGilmVRDv0dtw1xqJ5zNOgMpz5V06RPgIW5rDC0o3SM/p2VtnmWvaNnw1rup5svq2LmAbgBu6tz+M2h5L1Zq6khMtIvZ9ycRV5sjNdmtk5kH8aYnb+A0D9Rrk0ROTvFe9x0hZFgDmCfBNHo5FvhjqUbho2aE3i3qCyb0V/Mf1IozR+yHr1XSejGDxDtTg0LZXxYNz6kCH7JDyiWaE1dG0KxqNvBr7/Jvg8gsqvQmJVQO0EvReCAKIkyXdo1sALF5ZyPQ9y+MKYXiOU9Zwrln2D9mOFMes3XL5KkIn33ihL/cU/VzfiXVQAn/K4xwm9Gt1rIamKRhlAwjRTPZ6QKBWjZ1LXSJbeSLMEO8d9Q+yBgZlOO0CFsDVe/df33n3slj9AEblx9pzBDJXialT99gZ/pSY9OrFO9gKYvaz3zMzgHp7LOvpznY4TC4HdU3BdrXcDPX052awha/4PDtsLwmDEqDTVEB3SBRAYQBxc9I1eCUl2YkfjoJSFVM6GTLxM2WBqXWp0D2aRttS7BD8I9xMv/4IV2wRPOVga7jnB9uKc9lyPZrP8+Rcg3j5GTmof+Jvqs91H2aCVOFoZhkqU7WIHVLlIHWzY44dy3ODRtRBbx3OPlnOFsGR4b9o/oXVJY73nxfbbD44SO+nKYXfg4lBdrCrfFt2+QJw+D+pnDJqBbJWSbj5igZ+9PRL+E6VELWBjrxRHUaMTa/lFCXzPIp2TxBXzsTAnT4upBIao29jHeT7EXxPbi5ifN8Bx1utwACXZdJtV1uvWuFcjpQ3mAFGwC1CCQT58CPJH0YRmBrfeYXAKvN2rMzdjyjQJNqNEuyBT7TUdqQ0rHCI7K8kXmCaXIFceO9tHe+nLUxsjkrTu1iv72QfQnFOL92VEGnE4/cu2kGsobyv5MBhc1AIetUshRzoGK4wmpx95blxwhHQERCHMHwW0zoWLMT1py/yNIwe04kCEAK7jZyQyFYG8Axj5Wuc+KC49OtBIUGRQMfVvhCMmAN4bnKgf/2SYT6
            // found at https://www.reddit.com/r/adventofcode/comments/1pity70/comment/ntgnrqj/
            var ctx = new Context();
            var buttonVars = m.Buttons.Index().Select(x => ctx.MkIntConst($"button_{x.Index}")).ToArray();
            
            var opt = ctx.MkOptimize();
            foreach (var v in buttonVars)
                opt.Add(v >= 0);

            for (var j = 0; j < m.Joltage.Count; j++)
            {
                List<ArithExpr> contributing = [];

                for (var k = 0; k < m.Buttons.Count; k++)
                {
                    var button = m.Buttons[k];
                    if (button.Contains(j))
                        contributing.Add(buttonVars[k]);
                }

                var sumExpr = contributing.Aggregate(ctx.MkInt(0) + ctx.MkInt(0), (a, b) => a + b);
                opt.Add(ctx.MkEq(sumExpr, ctx.MkInt(m.Joltage[j])));
            }

            opt.MkMinimize(buttonVars.Aggregate(ctx.MkInt(0) + ctx.MkInt(0), (a, b) => a + b));

            opt.Check();
            var model = opt.Model;
            
            sum += buttonVars.Sum(x => (long)model.Double(x));
        }

        return sum; // 16513
    }
}