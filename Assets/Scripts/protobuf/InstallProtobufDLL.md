## 记录一下window下unity项目中使用protobuf的方法

### 在vs的NuGet包管理器中下载Google.Protobuf
### 找到C:\Users\<用户名>\.nuget\packages

### 以及四个dll文件
#### packages\Google.Protobuf\3.28.3\lib\net45\Google.Protobuf.dll

#### packages\System.Buffers\4.4.0\lib\netstandard2.0\System.Buffers.dll

#### packages\System.Memory\4.5.3\lib\netstandard2.0\System.Memory.dll

#### packages\System.Runtime.CompilerServices.Unsafe\4.5.2\lib\netstandard2.0\ System.Runtime.CompilerServices.Unsafe.dll 

### 将这四个dll文件拷贝到unity项目的Assets\Plugins目录下