using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial - string.Split
| Method | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------- |---------:|--------:|--------:|-------:|-------:|----------:|
| Part1  | 459.6 us | 1.71 us | 1.60 us | 4.8828 | 0.4883 |   42.7 KB |
| Part2  | 174.6 us | 1.22 us | 1.02 us | 5.1270 | 0.4883 |   42.7 KB |

Span2D
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 452.5 us | 5.28 us | 4.94 us |      24 B |
| Part2  | 160.3 us | 0.90 us | 0.80 us |      24 B |

Rework part 1
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 170.3 us | 0.53 us | 0.47 us |      24 B |
| Part2  | 161.3 us | 0.50 us | 0.47 us |      24 B |

Part1: Use StartsWith for horizontal case
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 161.3 us | 0.39 us | 0.34 us |      24 B |
| Part2  | 163.3 us | 1.36 us | 1.27 us |      24 B |

Part1: Get rid of bounds checks
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 146.5 us | 0.48 us | 0.43 us |      24 B |
| Part2  | 163.4 us | 1.24 us | 1.16 us |      24 B |

Rework part2, DangerousGetReferenceAt in part1
| Method | Mean      | Error    | StdDev   | Allocated |
|------- |----------:|---------:|---------:|----------:|
| Part1  | 152.50 us | 1.175 us | 1.099 us |      24 B |
| Part2  |  20.07 us | 0.057 us | 0.048 us |      24 B |
 */
public class Day04 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 4;
    
    protected override object Part1Impl()
    {
        var input = Input.Text;
        var lineWidth = input.IndexOf('\n') + 1;
        var span = ReadOnlySpan2D<char>.DangerousCreate(input.AsSpan()[0], input.Length / lineWidth + 1, lineWidth, 0);
        var height = span.Height;
        var count = 0;

        for (int y = 0; y < height; y += 1)
        {
            for (int x = 0; x < lineWidth - 1; x += 1) // -1 as we're skipping the newline character
            {
                var at = span[y, x];
                if (at is not ('X' or 'S'))
                    continue;

                if (x < lineWidth - 3)
                {
                    // can move right
                    var row = span.GetRowSpan(y)[(x+1)..];
                    if (at switch
                        {
                            'X' => row.StartsWith("MAS"),
                            _ => row.StartsWith("AMX"),
                        })
                        count++;
                    if (y < height - 3 && CheckDiagonal(span, x, y, 1, 1, at))
                        count++;
                    if (y > 2 && CheckDiagonal(span, x, y, 1, -1, at))
                        count++;
                }
                
                if (y < height - 3 && CheckDiagonal(span, x, y, 0, 1, at))
                    count++;
            }
            
            static bool CheckDiagonal(ReadOnlySpan2D<char> span, int x, int y, int stepX, int stepY, char first)
            {
                if (first == 'X')
                {
                    // search for 'MAS'
                    return span.DangerousGetReferenceAt(y += stepY, x += stepX) is 'M'
                           && span.DangerousGetReferenceAt(y += stepY, x += stepX) is 'A'
                           && span.DangerousGetReferenceAt(y + stepY, x + stepX) is 'S';
                }
                if (first == 'S')
                {
                    // search for 'AMX'
                    return span.DangerousGetReferenceAt(y += stepY, x += stepX) is 'A'
                           && span.DangerousGetReferenceAt(y += stepY, x += stepX) is 'M'
                           && span.DangerousGetReferenceAt(y + stepY, x + stepX) is 'X';
                }

                return false;
            }
        }
        
        return count; // 2618
    }

    protected override object Part2Impl()
    {
        var input = Input.Text;
        var lineWidth = input.IndexOf('\n') + 1;
        var span = ReadOnlySpan2D<char>.DangerousCreate(input.AsSpan()[0], input.Length / lineWidth + 1, lineWidth, 0);
        var count = 0;
        var height = span.Height;
            
        for (var x = 1; x < lineWidth - 2; x += 1) // -2 as we're skipping the newline character
        {
            for (var y = 1; y < height - 1; y += 1)
            {
                var at = span[y, x];
                if (at == 'A')
                {
                    var topLeft = span.DangerousGetReferenceAt(y - 1, x - 1);
                    if (topLeft is not ('M' or 'S'))
                        continue;
                    var topRight = span.DangerousGetReferenceAt(y - 1, x + 1);

                    count += (topLeft, topRight) switch
                    {
                        ('M', 'M') => span.DangerousGetReferenceAt(y + 1, x + 1) is 'S'
                                                && span.DangerousGetReferenceAt(y + 1, x - 1) is 'S',
                        ('M', 'S') => span.DangerousGetReferenceAt(y + 1, x + 1) is 'S'
                                                && span.DangerousGetReferenceAt(y + 1, x - 1) is 'M',
                        ('S', 'S') => span.DangerousGetReferenceAt(y + 1, x + 1) is 'M'
                                                && span.DangerousGetReferenceAt(y + 1, x - 1) is 'M',
                        ('S', 'M') => span.DangerousGetReferenceAt(y + 1, x + 1) is 'M'
                                                && span.DangerousGetReferenceAt(y + 1, x - 1) is 'S',
                        _ => false,
                    } ? 1 : 0;
                }
            }
        }
        
        return count; //2011
    }
}