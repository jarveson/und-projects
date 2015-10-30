program MyTest(input, output);
var
    jake, hi,bye:integer;
    ran:char;
begin
    write("Enter in a number: ");
    read(hi);
    write("Enter in a char: ");
    read(ran);
    
    bye := 6;
    
    hi := (hi div bye) + 12;
    write("Number after some stuff is: ");
    writeln(hi);
    write("Char you entered was: ");
    writeln(ran);
end