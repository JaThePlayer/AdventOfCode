using System.Runtime.InteropServices;
using System.Text;

namespace AoC._2024;

public class Day17 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 17;
    
    protected override object Part1Impl()
    {
        ParseInput(out var a, out var b, out var c, out var program);
        
        List<byte> output = new();
        output = RunProgram(program, a, b, c, output, default);

        return string.Join(',', output); // 7,4,2,5,1,4,6,0,4
    }

    private void ParseInput(out long a, out long b, out long c, out ReadOnlySpan<byte> program)
    {
        var input = Input.TextU8;
        var lines = input.Split((byte)'\n');
        
        lines.MoveNext();
        a = Util.FastParseInt<int>(input[lines.Current]["Register A: ".Length..]);
        lines.MoveNext();
        b = Util.FastParseInt<int>(input[lines.Current]["Register A: ".Length..]);
        lines.MoveNext();
        c = Util.FastParseInt<int>(input[lines.Current]["Register A: ".Length..]);
        lines.MoveNext();
        lines.MoveNext();
        program = input[lines.Current]["Program: ".Length..];
    }

    private static List<byte> RunProgram(ReadOnlySpan<byte> program, long a, long b, long c, List<byte> output, ReadOnlySpan<byte> targetOutput)
    {
        var sa = a;
        var pc = 0;
        while (true)
        {
            if (pc * 2 >= program.Length)
                break;
            var opc = (byte)(program[pc*2] - '0');
            var operand = (byte)(program[pc*2 + 2] - '0');
            pc += 2;

            long GetCombo(byte operand)
            {
                switch (operand)
                {
                    case <= 3:
                        return operand;
                    case 4:
                        return a;
                    case 5:
                        return b;
                    case 6:
                        return c;
                    default:
                        throw new Exception($"Wrong combo: {operand}");
                }
            }

            switch (opc)
            {
                case 0: // adv
                    /*
                    The adv instruction (opcode 0) performs division. 
                    The numerator is the value in the A register. 
                    The denominator is found by raising 2 to the power of the instruction's combo operand. 
                    (So, an operand of 2 would divide A by 4 (2^2); an operand of 5 would divide A by 2^B.) 
                    The result of the division operation is truncated to an integer and then written to the A register.
                     */
                    a = (long)(a / Math.Pow(2, GetCombo(operand)));
                    break;
                case 6: // bdv
                    b = (long)(a / Math.Pow(2, GetCombo(operand)));
                    break;
                case 7: // cdv
                    c = (long)(a / Math.Pow(2, GetCombo(operand)));
                    break;
                case 1: // bxl
                    // The bxl instruction (opcode 1) calculates the bitwise XOR of register B and the instruction's literal operand,
                    // then stores the result in register B.
                    b = b ^ operand;
                    break;
                case 2: // bst
                    // The bst instruction (opcode 2) calculates the value of its combo operand modulo 8
                    // (thereby keeping only its lowest 3 bits),
                    // then writes that value to the B register.
                    b = GetCombo(operand) % 8;
                    break;
                case 3: // jnz
                    // The jnz instruction (opcode 3) does nothing if the A register is 0.
                    // However, if the A register is not zero, it jumps by setting the instruction pointer to the value of its literal operand;
                    // if this instruction jumps, the instruction pointer is not increased by 2 after this instruction.
                    if (a == 0)
                        break;
                    pc = operand;
                    break;
                case 4: // bxc
                    // The bxc instruction (opcode 4) calculates the bitwise XOR of register B and register C,
                    // then stores the result in register B.
                    // (For legacy reasons, this instruction reads an operand but ignores it.)
                    b = b ^ c;
                    break;
                case 5: // out
                    // The out instruction (opcode 5) calculates the value of its combo operand modulo 8,
                    // then outputs that value. (If a program outputs multiple values, they are separated by commas.)
                    byte toOutput = (byte)(GetCombo(operand) % 8);
                    if (targetOutput != default)
                    {
                        if (targetOutput[output.Count] != toOutput)
                            return output;
                       // Console.WriteLine($"Success: {output.Count}: {sa}");
                    }
                    
                    output.Add(toOutput);
                    break;
            }
        }

        //Console.WriteLine($"a={a};b={b};c={c}");
        return output;
    }

    protected override object Part2Impl()
    {
        //return -1;
        ParseInput(out _, out var bS, out var cS, out var program);
        var buffer = new List<byte>(program.Length);
        var target = program.ToArray().Where(x => x != ',').Select(x => (byte)(x - '0')).ToArray();


        for (int a = 0; a < 8; a++)
        {
            long b = a % 8;
            b = b ^ 1;
            var c = a / (long)(Math.Pow(2, b));
            b = b ^ 5;
            b = b ^ c;
            var o = (b % 8);
            Console.WriteLine((a, o));
        }

        return -1;
        /*
        long i = 1;
        long increment = 1;
        var prevCount = 0;
        while (buffer.Count != target.Length)
        {
            for (; i < long.MaxValue; i += increment)
            {
                buffer.Clear();
                RunProgram(program, i, b, c, buffer, target);
                var successCount = buffer.Count;
                if (successCount > prevCount)
                {        

                    //increment *= 8;//(long)Math.Pow(8, buffer.Count);
                    prevCount = buffer.Count;
                   // i = (long)Math.Pow(8, buffer.Count+1);
                    //increment = (long)Math.Pow(8, long.Max(1, buffer.Count - 1));
                    Console.WriteLine($"MORE[{buffer.Count}]: {i}, inc: {increment}");
                    break;
                } else if (successCount < prevCount)
                {
                    //Console.WriteLine($"FAIL: {i}");
                }
            }
            
            
        }
        */
        /*
        for (i = 0; i < 8; i++)
        {
            RunProgram(program, i, b, c, buffer, target);
            var successCount = buffer.Count;
            if (successCount > 0)
                break;
        }
        var increment = i;
        
        for (; i < 8*8; i += increment)
        {
            RunProgram(program, i, b, c, buffer, target);
            var successCount = buffer.Count;
            if (successCount > 1)
                break;
        }
*/
        
        // > 71300000000
        //   88645130703064
        
        /*
        var max = (long)Math.Pow(8, 16);
        for (long i = (long)Math.Pow(8, 15); i <= max; i += 1)
        {
            if (i % 100000000 == 0)
                Console.WriteLine($"Running a = {i}");
            RunProgram(program, i, b, c, buffer, target);
            
            var bufferSpan = CollectionsMarshal.AsSpan(buffer);
            if (bufferSpan.SequenceEqual(target))
                return i;
            
            var successCount = buffer.Count;
            
            buffer.Clear();
        }
        */
        
        return -1;
    }
}