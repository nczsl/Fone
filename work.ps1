param(
	[ValidateSet('work','netlibs','soalayer')][string]$proj='work',
	[switch]$build,
	[string]$class,
	[string]$name
)
if($name -eq '' -or $class -eq ''){
	'work tool test driven your project item'
	return;
}
[string]$workDir = Split-Path -Parent $PSScriptRoot
[string]$proj_path=''
[string]$default_namespace=''
switch($proj){
	"work" {
		$proj_path="$workDir\bofu\Bizallview.Test\Bizallview.Test.csproj"
		$default_namespace="Bizallview.Test"
	}
	"netlibs" {
		$proj_path="$workDir\core\Netlibs.Test\Netlibs.Test.csproj"
		$default_namespace="Netlibs.Test"
	}
	"soalayer" {
		$proj_path="$workDir\soa\SoaLayer.Test\SoaLayer.Test.csproj"
		$default_namespace="SoaLayer.Test"
	}
}
if($build){
dotnet test --filter "Name=$name&ClassName=$default_namespace.$class" --logger 'console;verbosity=detailed' $proj_path
}else{
dotnet test --filter "Name=$name&ClassName=$default_namespace.$class" --logger 'console;verbosity=detailed' --no-build $proj_path
}
