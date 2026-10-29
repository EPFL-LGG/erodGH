function Get-ErodFileHash {
    param([Parameter(Mandatory=$true)][string]$Path,
          [ValidateSet('SHA256','SHA512')][string]$Algorithm='SHA256')
    $stream=[IO.File]::OpenRead($Path)
    $hasher=[Security.Cryptography.HashAlgorithm]::Create($Algorithm)
    try {
        [pscustomobject]@{Hash=([BitConverter]::ToString($hasher.ComputeHash($stream))).Replace('-','')}
    } finally {
        $hasher.Dispose()
        $stream.Dispose()
    }
}
