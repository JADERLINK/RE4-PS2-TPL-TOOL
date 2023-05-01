# RE4-PS2-TPL-TOOL
Extract and repack ps2 re4 tpl files

## Info:
License: MIT Licence
<br>Linguage: C#
<br>Platform: Windows
<br>Dependency: Microsoft .NET Framework 4.8

## **Translate from Portuguese Brazil:**

Programa destinado a extrair as imagens do arquivo tpl da versão de PS2 do Re4.
<br>**Update: alfa.1.0.0.1:**
<br>Adicionado o programa TPL_PS2_REPACK, ainda não esta 100% concluido, mas já pode ser usado, veja as notas na seção específica.

<br>
---

**Código de terceiro:**

[TGASharpLib by ALEXGREENALEX](https://github.com/ALEXGREENALEX/TGASharpLib).

-----
<br>

# TPL_PS2_EXTRACT
Destinado a extrair os arquivos Tpl, Por exemplo: com o arquivo "idm000.tpl" será gerado um arquivo .idxtpl (idm000.idxtpl), e as imagens serão colocadas em uma pasta com o nome "Textures".

# TPL_PS2_REPACK
Destinado a recompilar o arquivo Tpl, o arquivo usado para recompilar o tpl é o "*.idxtpl".
<br> Notas: 
* O suporte para paleta de mipmap não foi implementado, então as imagens dos mipmaps devem ter as mesmas cores das images principais.
* Caso for imagens de bitdepth 0x8 ou 0x9, será usado somente as primeiras 16/256 cores presentes na imagem, qualquer cor a mais, sera descartada, e preenchida  com a cor do primeiro ou ultimo indice da paleta.
* Condições referente aos tipos de imagens: bitdepth/interlace:
    - bitdepth: 0x8, interlace 0x0 ou 0x1: será aceito qualquer imagem de tamanho par, tamanho impares não serão compiladas, você deve garantir que a imagem tenha somente 16 cores.
    - bitdepth: 0x9, interlace 0x0 ou 0x1: será aceito imagens de qualquer tamanho, você deve garantir que a imagem tenha somente 256 cores.
    - bitdepth: 0x6, interlace 0x0 ou 0x1: aceita imagens de qualquer tamanho, e de qualquer quantiade de cor, Aviso: esse formato é o qual ocupa mais espaço.
    - bitdepth: 0x8, interlace 0x2 ou 0x3: aceita somente imagens de potências de 2, a partir da resolução 128x128, (tamanhos inferiores a esse não serão compilados)
    - bitdepth: 0x9, interlace 0x2 ou 0x3: aceita somente imagens de potências de 2, a partir da resolução 32x16, (tamanhos inferiores a esse não serão compilados)
    - bitdepth: 0x6, interlace 0x2 ou 0x3: não suportado, não use. 
*   Caso for usar uma imagem de tamanho diferente do tamanho original, o conteúdo de "\*_unk5", "\*_unk7", "\*_unkx28" e "\*unkx2C" devem ser alterados de acordo com a resolução da imagem.

-----
**At.te: JADERLINK**

2023-04-16