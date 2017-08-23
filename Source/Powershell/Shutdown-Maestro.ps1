function Shutdown-Maestro
{
	$credPass = ConvertTo-SecureString 'Sq5!@MZJUrW9cf8' -AsPlainText -Force
	$cred = New-Object System.Management.Automation.PSCredential ('crossmw\maestro', $credPass)

	$session = New-PSSession -ComputerName "10.24.247.15" -Credential $cred

    ##runs maestro kill switch, then stops all of the services.

    Invoke-Command -Session $session -Scriptblock {
    Start-Process -FilePath 'C:\Maestro\Services\MaestroKillSwitch\MaestroKillSwitch.exe' -Wait

    Stop-Process -Name Tam.Maestro.Services.Accounting.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Affidavits.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.AudienceAndRatings.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Broadcast.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.CmwTraffic.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.CoverageUniverse.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Delivery.Driver -Force
    #Stop-Process -Name Tam.Maestro.Services.Emailer.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Inventory.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Materials.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Posting.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.PostLogs.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Proposals.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.RateCards.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Releases.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Releases2.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Reporting.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.SystemsTopography.Driver -Force
    Stop-Process -Name Tam.Maestro.Services.Traffic.Driver -Force
	Stop-Process -Name Tam.Maestro.Services.ServiceManager.Driver -Force
	Stop-Process -Name Tam.Maestro.Services.BusinessObjectsManager.Driver -Force
	Stop-Process -Name Tam.Maestro.Services.MaestroAdministration.Driver -Force
	Stop-Process -Name SemanticLogging-svc -Force
	}

	Remove-PSSession $session
}

Shutdown-Maestro
