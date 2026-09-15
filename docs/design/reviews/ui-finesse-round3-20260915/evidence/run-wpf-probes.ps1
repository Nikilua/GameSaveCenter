param([Parameter(Mandatory=$true)][string]$PluginAssemblyPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $PluginAssemblyPath).Path)
$flags = [Reflection.BindingFlags]'Static, Public, NonPublic'
$motion = $assembly.GetType('GameSaveCenter.Playnite.Infrastructure.GscMotion', $true)
$scaleMethod = $motion.GetMethod('GetMutableScaleTransform', $flags)
$element = [System.Windows.Controls.Border]::new()
$element.RenderTransform = [System.Windows.Media.TranslateTransform]::new(3,4)
$first = $scaleMethod.Invoke($null, @($element))
$second = $scaleMethod.Invoke($null, @($element))
$third = $scaleMethod.Invoke($null, @($element))
"Scale reused: $([Object]::ReferenceEquals($first,$second))"
"Root transform: $($element.RenderTransform.GetType().Name); child0: $($element.RenderTransform.Children[0].GetType().Name); child0.child0: $($element.RenderTransform.Children[0].Children[0].GetType().Name)"
$one = [System.Windows.Controls.Border]::new()
$two = [System.Windows.Controls.Border]::new()
$shared = [System.Windows.Media.TranslateTransform]::new()
$one.RenderTransform = $shared
$two.RenderTransform = $shared
$translation = $motion.GetMethod('GetMutableTranslateTransform', $flags).Invoke($null,@($one))
$translation.X = 42
"Shared mutable transform sibling X: $($two.RenderTransform.X)"
$entry = [System.Windows.Controls.Border]::new()
$motion.GetMethod('AnimateEntrance', $flags).Invoke($null,@($entry,[double]12))
$entry.BeginAnimation([System.Windows.UIElement]::OpacityProperty,$null)
$entry.RenderTransform.BeginAnimation([System.Windows.Media.TranslateTransform]::YProperty,$null)
"Entrance after clock removal: opacity=$($entry.Opacity); Y=$($entry.RenderTransform.Y)"
$typo = $assembly.GetType('GameSaveCenter.Playnite.Infrastructure.TypographyDiagnostics',$true)
$glyphArgs = @('__GSC_FONT_DOES_NOT_EXIST_72618__',[int]65,[System.Windows.FontWeights]::Normal,[System.Windows.FontWeights]::Normal)
$hasGlyph = $typo.GetMethod('TryGetGlyph',$flags).Invoke($null,$glyphArgs)
"Nonexistent font reported glyph A: $hasGlyph"
$guard = $assembly.GetType('GameSaveCenter.Playnite.Infrastructure.AdaptiveThemePaletteContrastGuard',$true)
$black = [System.Windows.Media.Colors]::Black
$white = [System.Windows.Media.Colors]::White
$clear = [System.Windows.Media.Colors]::Transparent
$colorArray = [System.Windows.Media.Color[]]@($white)
$gradientArgs = @('probe',$black,$black,$colorArray,$clear,$clear,[double]0.5,[double]4.5)
$measurements = $guard.GetMethod('MeasureGradientTextContrast',$flags).Invoke($null,$gradientArgs)
$pressed = $measurements | Where-Object Check -EQ 'probe.pressed@0.0'
"Pressed 0.5 black text / white chrome / black backdrop: reported background=$($pressed.Background); foreground=$($pressed.EffectiveForeground); ratio=$($pressed.Actual)"
"Expected composition: background=#808080; foreground=#000000 (entire chrome and text group at opacity 0.5)"
