using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GameSaveCenter.Playnite.Controls;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class StatusGlyphConverterTests
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        [Theory]
        [InlineData("成功", "✓ 成功")]
        [InlineData("已锁定", "✓ 已锁定")]
        [InlineData("备份失败", "× 备份失败")]
        [InlineData("待处理", "⚠ 待处理")]
        [InlineData("设备冲突", "⚠ 设备冲突")]
        public void KnownStatusesReceiveAccessibleTextCue(string status, string expected)
        {
            var converter = new StatusGlyphConverter();

            Assert.Equal(expected, converter.Convert(status, typeof(string), string.Empty, Culture));
        }

        [Theory]
        [InlineData("自定义状态")]
        [InlineData("✓ 已完成")]
        [InlineData("")]
        public void UnknownOrAlreadyDecoratedStatusesPassThrough(string status)
        {
            var converter = new StatusGlyphConverter();

            Assert.Equal(status, converter.Convert(status, typeof(string), string.Empty, Culture));
        }

        [Fact]
        public void ConvertBackNeverWritesBusinessState()
        {
            var converter = new StatusGlyphConverter();

            Assert.Same(Binding.DoNothing, converter.ConvertBack("✓ 成功", typeof(string), string.Empty, Culture));
        }
    }
}
