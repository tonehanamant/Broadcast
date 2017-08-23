function Start-MaestroServicesOnDeck
{
    $credPass = ConvertTo-SecureString 'Sq5!@MZJUrW9cf8' -AsPlainText -Force
	$cred = New-Object System.Management.Automation.PSCredential ('crossmw\maestro', $credPass)

	$session = New-PSSession -ComputerName "10.24.247.15" -Credential $cred

	Invoke-Command -Session $session -Scriptblock {
	Start-Process -FilePath C:\Maestro\Startup_OnDeck.bat -Verb runasS
	}
	Remove-PSSession $session
}

Start-MaestroServicesOnDeck