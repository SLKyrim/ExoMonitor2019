FS=1000;
Wc=2*30/FS;
[b,a]=butter(2,Wc);
akl_filter=filtfilt(b,a,[akl',zeros(1,10)])'; 
ahl_filter=filtfilt(b,a,[ahl',zeros(1,10)])';
ahr_filter=filtfilt(b,a,[ahr',zeros(1,10)])';
akr_filter=filtfilt(b,a,[akr',zeros(1,10)])';
data=[akl_filter,ahl_filter,ahr_filter,akr_filter];


[r,c]=size(data); 
fid=fopen(['����.txt'],'w'); 
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

fid=fopen(['��ʼ��������.txt'],'w'); 
for i=1:101
    for j=1:4
        fprintf(fid,'%f\t',data(i,j));
    end
    if i == 101
        continue;
    else
        fprintf(fid,'\n');
    end 
end
fclose(fid);

fid=fopen(['����ʼ����������������.txt'],'w'); 
for i=101:201
    for j=1:4
        fprintf(fid,'%f\t',data(i,j));
    end
    if i == 201
        continue;
    else
        fprintf(fid,'\n');
    end 
end
fclose(fid);

fid=fopen(['������������.txt'],'w');
for i=201:301
    for j=1:4
        fprintf(fid,'%f\t',data(i,j));
    end
    if i == 301
        continue;
    else
        fprintf(fid,'\n');
    end 
end
fclose(fid);

fid=fopen(['����������������������.txt'],'w');
for i=301:401
    for j=1:4
        fprintf(fid,'%f\t',data(i,j));
    end
    if i == 401
        continue;
    else
        fprintf(fid,'\n');
    end 
end
fclose(fid);

fid=fopen(['�ӿ粽ǰ��������������.txt'],'w');
for i=401:501
    for j=1:4
        fprintf(fid,'%f\t',data(i,j));
    end
    if i == 501
        continue;
    else
        fprintf(fid,'\n');
    end 
end
fclose(fid);

fid=fopen(['Խ�ϲ��ղ�.txt'],'w');
for i=501:r
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