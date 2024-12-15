using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial
| Method | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------- |-----------:|--------:|--------:|-------:|----------:|
| Part1  |   167.0 us | 1.15 us | 1.02 us | 0.2441 |   2.54 KB |
| Part2  | 1,065.6 us | 9.20 us | 8.16 us |      - |   9.84 KB |
 */
public class Day15 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 15;
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        var splitIdx = input.IndexOf([(byte)'\n', (byte)'\n']);

        var map1d = input[..splitIdx].ToArray();
        var width = map1d.AsSpan().IndexOf((byte)'\n') + 1;

        var map = Span2D<byte>.DangerousCreate(ref map1d[0], map1d.Length / width + 1, width, 0);

        var inputs = input[(splitIdx + 2)..];
        var guardIdx = input.IndexOf((byte)'@');
        var (sgx, sgy) = (guardIdx % map.Width, guardIdx / map.Width);
        map[sgy, sgx] = (byte)'.';
        
        var (gx, gy) = (sgx, sgy);

        for (int i = 0; i < inputs.Length; i++)
        {
            switch (inputs[i])
            {
                case (byte)'<':
                    TryMove(map, ref gx, ref gy, -1, 0);
                    break;
                case (byte)'>':
                    TryMove(map, ref gx, ref gy, 1, 0);
                    break;
                case (byte)'^':
                    TryMove(map, ref gx, ref gy, 0, -1);
                    break;
                case (byte)'v':
                    TryMove(map, ref gx, ref gy, 0, 1);
                    break;
            }

            //Console.WriteLine($"After {i}: ------");
            //PrintBoard(map, gx, gy);
        }

        // gps

        ulong sum = 0;

        for (int y = 1; y < map.Height - 1; y++)
        {
            var row = map.GetRowSpan(y);
            for (int x = 1; x < map.Width - 2; x++)
            {
                if (row[x] == 'O')
                {
                    sum += (ulong)(y * 100 + x);
                }
            }
        }

        return sum; // 1437174
        
        bool TryMove(Span2D<byte> map, ref int x, ref int y, int ox, int oy)
        {
            var nx = x + ox;
            var ny = y + oy;

            ref var nextTile = ref map[ny, nx];
            if (nextTile == '#')
                return false;
            if (nextTile == 'O')
            {
                // box
                var (nnx, nny) = (nx, ny);
                if (TryMove(map, ref nnx, ref nny, ox, oy))
                {
                    nextTile = (byte)'.';
                    map[nny, nnx] = (byte)'O';
                    x = nx;
                    y = ny;
                    return true;
                }

                return false;
            }

            x = nx;
            y = ny;
            return true;
        }
    }

    private static void PrintBoard(Span2D<byte> map, int gx, int gy)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width - 1; x++)
            {
                if (gx == x && gy == y)
                    Console.Write('@');
                else
                    Console.Write((char)map[y, x]);
            }

            Console.WriteLine();
        }
    }

    protected override object Part2Impl()
    {
        var input = Input.TextU8;
        var splitIdx = input.IndexOf([(byte)'\n', (byte)'\n']);

        
        var origWidth = input.IndexOf((byte)'\n') + 1;
        var mapOrig = ReadOnlySpan2D<byte>.DangerousCreate(input[0], splitIdx / origWidth + 1, origWidth, 0);
        
        
        var width = (mapOrig.Width-1) * 2;
        var map1d       = new byte[mapOrig.Height * width];
        var mapBackup1d = new byte[mapOrig.Height * width];
        
        var map       = Span2D<byte>.DangerousCreate(ref map1d[0], mapOrig.Height, width, 0);
        var mapBackup = Span2D<byte>.DangerousCreate(ref mapBackup1d[0], mapOrig.Height, width, 0);
        
        for (int y = 0; y < mapOrig.Height; y++)
        {
            var origRow = mapOrig.GetRowSpan(y);
            var newRow = map.GetRowSpan(y);
            for (int x = 0; x < mapOrig.Width - 1; x++)
            {
                var nx = x * 2;
                switch (origRow[x])
                {
                    case (byte)'#':
                        newRow[nx] = (byte)'#';
                        newRow[nx+1] = (byte)'#';
                        break;
                    case (byte)'O':
                        newRow[nx] = (byte)'[';
                        newRow[nx+1] = (byte)']';
                        break;
                    case (byte)'.' or (byte)'@':
                        newRow[nx] = (byte)'.';
                        newRow[nx+1] = (byte)'.';
                        break;
                }
            }
        }
        

        var inputs = input[(splitIdx + 2)..];
        var guardIdx = input.IndexOf((byte)'@');
        var (sgx, sgy) = (guardIdx % mapOrig.Width, guardIdx / mapOrig.Width);
        sgx *= 2;
        map[sgy, sgx] = (byte)'.';
        
        var (gx, gy) = (sgx, sgy);
        //PrintBoard(map, gx, gy);

        for (int i = 0; i < inputs.Length; i++)
        {
            map1d.AsSpan().CopyTo(mapBackup1d);
            switch (inputs[i])
            {
                case (byte)'<':
                    TryMoveHorizontal(map, ref gx, ref gy, -1);
                    break;
                case (byte)'>':
                    TryMoveHorizontal(map, ref gx, ref gy, 1);
                    break;
                case (byte)'^':
                    if(!TryMoveVertical(map, ref gx, ref gy, -1))
                        mapBackup1d.AsSpan().CopyTo(map1d);
                    break;
                case (byte)'v':

                    if(!TryMoveVertical(map, ref gx, ref gy, 1))
                        mapBackup1d.AsSpan().CopyTo(map1d);
                    break;
            }

            //Console.WriteLine($"After {i}: ------");
            //PrintBoard(map, gx, gy);
        }

        // gps

        ulong sum = 0;

        for (int y = 1; y < map.Height - 1; y++)
        {
            var row = map.GetRowSpan(y);
            for (int x = 1; x < map.Width - 2; x++)
            {
                if (row[x] == '[')
                {
                    sum += (ulong)(y * 100 + x);
                }
            }
        }

        //PrintBoard(map, gx, gy);
        return sum; // 1437468
        
        bool TryMoveHorizontal(Span2D<byte> map, ref int x, ref int y, int ox)
        {
            var nx = x + ox;

            ref var nextTile = ref map[y, nx];
            if (nextTile == '#')
                return false;
            if (nextTile is (byte)'[' or (byte)']')
            {
                // box
                var (nnx, nny) = (nx+ox, y);
                if (TryMoveHorizontal(map, ref nnx, ref nny, ox))
                {
                    map[nny, nnx] = ox == -1 ? (byte)'[' : (byte)']';
                    map[nny, nnx-ox] = nextTile;
                    nextTile = (byte)'.';
                    x = nx;
                    return true;
                }

                return false;
            }

            x = nx;
            return true;
        }
        
        bool TryMoveVertical(Span2D<byte> map, ref int x, ref int y, int oy)
        {
            var ny = y + oy;

            ref var nextTile = ref map[ny, x];
            if (nextTile == '#')
                return false;
            if (nextTile == ']')
            {
                // box
                var (nnxR, nnyR) = (x, ny);
                var (nnxL, nnyL) = (x-1, ny);
                if (TryMoveVertical(map, ref nnxL, ref nnyL, oy)
                 && TryMoveVertical(map, ref nnxR, ref nnyR, oy))
                {
                    map[nnyL, nnxL] = (byte)'[';
                    map[nnyR, nnxR] = (byte)']';
                    nextTile = (byte)'.';
                    map[ny, x-1] = (byte)'.';
                    y = ny;
                    return true;
                }
                return false;
            }
            if (nextTile == '[')
            {
                // box
                var (nnxR, nnyR) = (x+1, ny);
                var (nnxL, nnyL) = (x, ny);
                if (TryMoveVertical(map, ref nnxL, ref nnyL, oy)
                    && TryMoveVertical(map, ref nnxR, ref nnyR, oy))
                {
                    map[nnyL, nnxL] = (byte)'[';
                    map[nnyR, nnxR] = (byte)']';
                    nextTile = (byte)'.';
                    map[ny, x+1] = (byte)'.';
                    y = ny;
                    return true;
                }
                return false;
            }

            y = ny;
            return true;
        }
    }
}