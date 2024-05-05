# RE4-PS2-TPL-TOOL
Extract and repack ps2 re4 tpl files

## Info:
License: MIT Licence
<br>Linguage: C#
<br>Platform: Windows
<br>Dependency: Microsoft .NET Framework 4.8

**Translate from Portuguese Brazil:**

Programa destinado a extrair e recompactar as imagens do arquivo tpl da versão de PS2 do Re4.

**Update B.1.1.1**
<br>Código atualizado para ter compatibilidade com o "RE4_PS2_MODEL_VIEWER";
<br>Melhorado a velocidade do tool de extração de tpl;
<br>A tool de extração não suporta mais criar arquivo .gif, pois estava criando os arquivos com as cores erradas.

## Update B.1.1.0.0

O programa foi reestruturado, agora as imagens serão enumeradas e colocadas em uma pasta de mesmo nome do arquivo Tpl, e ao lado de cada imagem, vai ter um arquivo IdxtplHeader;

## RE4_PS2_TPL_EXTRACT.exe
Destinado a extrair os arquivos Tpl, Por exemplo: com o arquivo "idm000.tpl" será gerado um arquivo .idxps2tpl (idm000.idxps2tpl), e as imagens serão enumeradas e colocadas em uma pasta com o mesmo nome do tpl, que nesse exemplo será "idm000".
<br>Os nomes das imagens ficarão: 0000.tga, 0001.tga, 0002.tga, etc;
<br>E ao lado de cada imagem terá um arquivo de mesmo nome que a imagem com o formato "IdxtplHeader", nesse arquivo tem o conteúdo necessário para recompilar o "Header" da imagem. 

## RE4_PS2_TPL_REPACK.exe
Destinado a recompilar o arquivo Tpl, o arquivo usado para recompilar o tpl é o "*.idxps2tpl". Antes de recompactar, veja os tópicos abaixo referente a editar as imagens e o referente ao arquivo IdxtplHeader;

## Editando as imagens

Ao editar as imagens, duas coisas devem ser consideradas: a resolução da imagem e a quantidade de cores;
<br> * **Bitdepth 9**: são imagens de 256 cores (então sua imagem pode ter no máximo 256 cores);
<br> * **Bitdepth 8**: são imagens de 16 cores (então sua imagem pode ter no maximo 16 cores);
<br> * Nota: se na sua imagem tiver uma quantidade a mais de cores especificadas acima, ao fazer o repack essas cores serão descartadas e colocada qualquer outra cor no lugar;
<br> * **Bitdepth 6**: são imagens que permitem usar todas as cores, porém, o jogo quase não usa essas imagens, pois elas ocupam muito espaço e são pesadas para o jogo, então não recomendo usar esse tipo de imagem.

! Caso você queira mudar a resolução ou a densidade de cor, você deve editar o arquivo IdxtplHeader de mesmo nome que a imagem, para você preencher esses arquivo com os dados corretos, use os arquivos IdxtplHeader que estão dentro do arquivo "IdxtplHeader.zip" que esta disponível junto com os arquivos do programa, nesse zip vai ter todos os header conhecidos que você pode usar.

! Referente à quantidade de cores na imagem, você pode usar o programa "IrfanView" para reduzir a quantidade de cores da sua imagem;

## Interlace

Alem do campo "Bitdepth", outro campo importante é o "Interlace", que, na verdade esse campo é um conjunto de flags bit a bit, no qual são as seguintes flags:
<br>* **Rotate Image**: essa flag é destina a girar a imagem, pois no arquivo tpl todas as imagens estão "deitadas", isto é, a largura é maior que a altura. Para você ter a imagem "em pé" no qual a altura é maior que a largura, essa flag deve ser ativada.
<br>* **Swizzle**: essa flag define que a imagem está armazenada com o pixel em uma ordem diferente do padrão (tive um grande trabalho para extrair essas imagens da maneira correta), mas, na prática, você não vai ver diferença visual, supostamente essas imagens devem ser mais "performáticas" para o jogo;

Abaixo está a lista de combinações dessas flags, então, quando você ver esses valores, você vai saber quais flags estão ativas:
<br> * **0x00**: nenhuma flag ativa;
<br> * **0x01**: "Rotate Image";
<br> * **0x02**: "Swizzle";
<br> * **0x03**: "Swizzle" + "Rotate Image";

## IdxtplHeader

Esse arquivo contem o "header" de cada imagem, é nele que é definido o "Bitdepth" e o "Interlace", e também tem a resolução da imagem, além de ter os campos "Next", "Qwc", "GsTex", que no qual os valores desses campo, são preenchidos conforme a resolução e Bitdepth da imagem, então para preencher esses campo corretamente, use os arquivos IdxtplHeader que estão dentro de "IdxtplHeader.zip" como referência;
<br> Nota: o valor do campo "Width" é sempre maior ou igual que o campo "Height";
<br> Caso a sua imagem esteja em uma resolução diferente da do arquivo IdxtplHeader, a tool vai avisar.
<br> Aviso: o preenchimento incorreto do arquivo IdxtplHeader, pode levar o jogo a crachar ou exibir a imagem de maneira incorreta;

## Tpl de cenário e Mipmap

Nos Tpl dos cenários (arquivos SMD), tem imagem de mipmap, que são imagens de tamanho menor que a imagem principal, o header dos mipmap fica junto do IdxtplHeader da imagem principal;
<br> Os mipmap e a imagem principal, compartilham a mesma palheta de cores, então as 3 imagem devem ter as mesmas cores;
<br> As imagens dos Tpl de cenários devem estar sempre deitadas, pois nesse caso em específico o jogo não reconhece as imagens como se estive em pé; 

## idxps2tpl

Esse é o arquivo usado para recompilar o arquivo Tpl, nele tem os seguintes campos:
<br>* **ImageFlipY:** se verdadeiro, as imagens são fipladas em Y ao serem recompactadas, isto é usado para as imagens dos Tpl que são usados pelos BIN/SMD;
<br>* **ImageFolder:** aqui contem o nome da pasta que contém as imagens;
<br>* **ImageFormat:** aqui contem o formato das imagens que estão na pasta, normalmente é TGA;

## Código de terceiro:

[TGASharpLib by ALEXGREENALEX](https://github.com/ALEXGREENALEX/TGASharpLib).

**At.te: JADERLINK**
<br>2024-05-05
