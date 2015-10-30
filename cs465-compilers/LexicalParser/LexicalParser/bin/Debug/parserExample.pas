program MyTest(input, output);
var
    done, a, b, i,j:integer;
begin
    read(a);
    read(b);
    write(a);
    i := 0;
    if i = 0 then
        write(0)
    else if i = 1 then
        write(1)
    else if i = 2 then
        write(2)
    else write(99);
    
    i := 1;
    if i = 0 then
        write(0)
    else if i = 1 then
        write(1)
    else if i = 2 then
        write(2) then
    else write(99);
    
    i = 2;
    if i = 0 then
        write(0)
    else if i = 1 then
        write(1)
    else if i = 2 then
        write(2) then
    else write(99);
    
    done := 0;
    while done = 0 do begin
        read(i);
        if i = 0 then
            done := 1
        else begin
            write(i);
            i := fib(i);
            write(i)
        end (* else part *)
    (*writeln (" That is all folks! ")'*)
    end (* while *)
    
    jake := 12;
    hi := 12 + jake;
    bye := jake * hi
end