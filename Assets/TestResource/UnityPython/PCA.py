import numpy as np
import sys
import json
import scipy.linalg as sp

#/////////////////solv Ax=0
def null(A,eps=1e-15):
    u,s,vt= np.linalg.svd(A)
    null_space =np.compress(s<=eps,vt,axis= 0)
    return null_space.T
#////////////////////////////////////////////

path = 'D:/UnityProject/GitHub/CGAndCV/Assets/TestResource/UnityPython/point_position.json'
with open(path, 'r') as f:
    json_data =json.load(f)

print(json_data)

positionMatrix = np.array(json_data['position']).reshape(json_data['count'],3)
print(positionMatrix)
covarianceMatrix = np.cov(positionMatrix.T,bias=True)
eignArray = np.linalg.eig(covarianceMatrix)
# print (eignArray)
eigenValue = eignArray[0]
eignVectors = eignArray[1].T

index = np.argsort(eigenValue)
# print (index)

R = eignVectors[index[-1]]
S = eignVectors[index[-2]]
T = eignVectors[index[-3]]
# print(T)

RST= [R.tolist(),S.tolist(),T.tolist()]
# print (R,'\n',S,'\n',T,'\n')

#D= -N dot P 
# p1R = np.dot(positionMatrix[0],R)
# p2R = np.dot(positionMatrix[1],R)
# p3R = np.dot(positionMatrix[2],R)
# p4R = np.dot(positionMatrix[3],R)

minD_R = min( np.dot(positionMatrix,R))
maxD_R = max( np.dot(positionMatrix,R))
dirR =[minD_R,maxD_R]
# print( minD_R,maxD_R)

minD_S =min( np.dot(positionMatrix,S))
maxD_S =max( np.dot(positionMatrix,S))
dirS =[minD_S,maxD_S]
# print( minD_S,maxD_S)

minD_T =min( np.dot(positionMatrix,T))
maxD_T =max( np.dot(positionMatrix,T))
dirT=[ minD_T,maxD_T]
# print( minD_T,maxD_T)

json_res = {'RST':RST,'min_max_R':dirR,'min_max_S':dirS,'min_max_T':dirT}

path ='D:/UnityProject/GitHub/CGAndCV/Assets/TestResource/UnityPython/eigenRes.json'
with open (path, 'w') as f:
    json_data = json.dump(json_res,f,sort_keys='True',indent=4,separators=(',',':'))



