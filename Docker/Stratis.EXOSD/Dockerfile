FROM microsoft/dotnet:2.1-sdk

RUN git clone https://github.com/exoeconomy/EXOSFullNode.git \
    && cd /EXOSFullNode/src/Stratis.EXOSD \
	&& dotnet build
	
VOLUME /root/.exosnode

WORKDIR /EXOSFullNode/src/Stratis.EXOSD

COPY exos.conf.docker /root/.exosnode/exos/EXOSMain/exos.conf

EXPOSE 4562 4561 37223 38826

CMD ["dotnet", "run"]