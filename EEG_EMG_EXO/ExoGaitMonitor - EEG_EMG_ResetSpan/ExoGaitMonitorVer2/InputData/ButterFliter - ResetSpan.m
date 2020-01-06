FS=1000;
Wc=2*30/FS;
[b,a]=butter(2,Wc);
akl_filter=filtfilt(b,a,[zeros(1,10), akl', zeros(1,10)])'; 
ahl_filter=filtfilt(b,a,[zeros(1,10), ahl', zeros(1,10)])';
ahr_filter=filtfilt(b,a,[zeros(1,10), ahr', zeros(1,10)])';
akr_filter=filtfilt(b,a,[zeros(1,10), akr', zeros(1,10)])';
data=[akl_filter,ahl_filter,ahr_filter,akr_filter];


[r,c]=size(data); 
fid=fopen(['总体 - 复位越障.txt'],'w'); 
for i=1:r
    for j=1:4
        fprintf(fid,'%f\t',data(i,j));
    end
    if i == r
        continue;
    else
        fprintf(fid,'\n');
    end 
end
fclose(fid);

fid=fopen(['直接越障步态第一步.txt'],'w'); 
for i=1:111
    for j=1:4
        fprintf(fid,'%f\t',data(i,j));
    end
    if i == 111
        continue;
    else
        fprintf(fid,'\n');
    end 
end
fclose(fid);

fid=fopen(['直接越障步态收步.txt'],'w');
for i=111:r
    for j=1:4
        fprintf(fid,'%f\t',data(i,j));
    end
    if i == r
        continue;
    else
        fprintf(fid,'\n');
    end 
end
fclose(fid);