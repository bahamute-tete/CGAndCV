import sys

def add(a,b):
    return float(a)+float(b)

if __name__ == '__main__':
    #print('Hello World')
    print(add(sys.argv[1],sys.argv[2]))