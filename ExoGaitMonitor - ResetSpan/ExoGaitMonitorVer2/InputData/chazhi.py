# -*- coding: utf-8 -*-
"""
Spyder Editor

This is a temporary script file.
"""
import numpy as np


def cha(names):
    for name in names:
        f = open(name + ".txt","r")
        lines = f.readlines()

        data = []
        for line in lines:
            tmp = []
            for num in line.split():
                tmp.append(float(num))
            data.append(tmp)

        # 第一次插值
        richline = len(data) * 2 - 1
        rich1 = np.zeros((richline, 4))

        for i in range(richline):
            for j in range(4):
                if i % 2 == 0:
                    rich1[i][j] = data[i//2][j]
                else:
                    rich1[i][j] = (data[i//2 + 1][j] + data[i//2][j]) / 2

        # 第二次插值
        richline = len(rich1) * 2 -1
        rich2 = np.zeros((richline, 4))

        for i in range(richline):
            for j in range(4):
                if i % 2 == 0:
                    rich2[i][j] = rich1[i//2][j]
                else:
                    rich2[i][j] = (rich1[i//2 + 1][j] + rich1[i//2][j]) / 2

        import codecs
        with \
        codecs.open(name + "-扩展.txt",\
                    mode='w',encoding='utf-8') as file:
            for i in range(len(rich2)):
                for j in range(4):
                    if j < 3:
                        file.write(str(rich2[i][j]) + '\t')
                    else:
                        file.write(str(rich2[i][j]))
                if i < len(rich2) - 1:
                    file.write('\n')


if __name__ == '__main__':
    #names = ["起始步迈左腿","接起始步的正常步迈右腿","正常步迈左腿","接正常步的正常步迈右腿","接跨步前的正常步迈左腿",\
       #      "越障并收步","总体","越障起始步","越障第二步","越障收步"]
    # names = ["直接越障步态"]
    # names = ["总体"]
    names = ["直接越障步态第一步","直接越障步态收步"]
    cha(names)