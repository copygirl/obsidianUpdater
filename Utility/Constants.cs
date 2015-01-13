using System;

namespace obsidianUpdater.Utility
{
	public static class Constants
	{
		public static readonly int MONITOR_PORT = 52013;

		public static readonly string DATA_FILE = "obsidian.json";

		public static readonly string MONO_BIN = "mono";
		public static readonly string JAVA_BIN = "java";
		public static readonly string SERVER_STARTUP = "{JAVA_ARGS} -jar \"{JAR_FILE}\" {MC_ARGS}";

		public static readonly string DEFAULT_DIRECTORY = "server";
		public static readonly string DEFAULT_JAR_FILE = "minecraftforge.jar";
		public static readonly string DEFAULT_JAVA_ARGUMENTS =
			"-d64 -server -Xms2048m -Xmx4096m -XX:MaxPermSize=256m -XX:+UseParNewGC " +
			"-XX:+UseConcMarkSweepGC -XX:+CICompilerCountPerCPU -XX:+TieredCompilation " +
			"-Dlog4j.configurationFile={LOG_CONFIG}";
		public static readonly string DEFAULT_MINECRAFT_ARGUMENTS = "nogui";

		public static readonly string LOG_CONFIG_FILE = "log4j2.xml";
		public static readonly string LOG_CONFIG_DATA = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Configuration status=""WARN"" packages=""com.mojang.util"">
    <Appenders>
        <Console name=""FmlSysOut"" target=""SYSTEM_OUT"">
            <PatternLayout pattern=""[%d{HH:mm:ss}] [%t/%level] [%logger]: %msg%n"" />
        </Console>
        <Console name=""SysOut"" target=""SYSTEM_OUT"">
            <PatternLayout pattern=""[%d{HH:mm:ss}] [%t/%level]: %msg%n"" />
        </Console>
        <Queue name=""ServerGuiConsole"">
            <PatternLayout pattern=""[%d{HH:mm:ss}] [%t/%level] [%logger]: %msg%n"" />
        </Queue>
        <RollingRandomAccessFile name=""File"" fileName=""logs/latest.log"" filePattern=""logs/%d{yyyy-MM-dd}-%i.log.gz"">
            <PatternLayout pattern=""[%d{HH:mm:ss}] [%t/%level]: %msg%n"" />
            <Policies>
                <TimeBasedTriggeringPolicy />
                <OnStartupTriggeringPolicy />
            </Policies>
        </RollingRandomAccessFile>
        <Routing name=""FmlFile"">
            <Routes pattern=""$${ctx:side}"">
                <Route>
                    <RollingRandomAccessFile name=""FmlFile"" fileName=""logs/fml-${ctx:side}-latest.log"" filePattern=""logs/fml-${ctx:side}-%i.log"">
                        <PatternLayout pattern=""[%d{HH:mm:ss}] [%t/%level] [%logger/%X{mod}]: %msg%n"" />
                        <DefaultRolloverStrategy max=""3"" fileIndex=""max"" />
                        <Policies>
                            <OnStartupTriggeringPolicy />
                        </Policies>
                    </RollingRandomAccessFile>
                </Route>
                <Route key=""$${ctx:side}"">
                    <RandomAccessFile name=""FmlFile"" fileName=""logs/fml-junk-earlystartup.log"" >
                        <PatternLayout pattern=""[%d{HH:mm:ss}] [%t/%level] [%logger]: %msg%n"" />
                    </RandomAccessFile>
                </Route>
            </Routes>
        </Routing>
    </Appenders>
    <Loggers>
        <Logger level=""info"" name=""com.mojang"" additivity=""false"">
            <AppenderRef ref=""SysOut"" level=""INFO"" />
            <AppenderRef ref=""File"" />
            <AppenderRef ref=""ServerGuiConsole"" level=""INFO"" />
        </Logger>
        <Logger level=""info"" name=""net.minecraft"" additivity=""false"">
            <filters>
                <MarkerFilter marker=""NETWORK_PACKETS"" onMatch=""DENY"" onMismatch=""NEUTRAL"" />
            </filters>
            <AppenderRef ref=""SysOut"" level=""INFO"" />
            <AppenderRef ref=""File"" />
            <AppenderRef ref=""ServerGuiConsole"" level=""INFO"" />
        </Logger>
        <Root level=""all"">
            <AppenderRef ref=""FmlSysOut"" />
            <AppenderRef ref=""ServerGuiConsole"" level=""INFO"" />
            <AppenderRef ref=""FmlFile""/>
        </Root>
    </Loggers>
</Configuration>
";
	}
}

