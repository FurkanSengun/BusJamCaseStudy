**Architecture**

Rollic Game Developer Case için oluşturduğum bu "Bus Jam Clone" projesinde, ana hedefim Dependency Injection yani "Extenject" kullanarak, kodların olabildiğince az birbirine bağlı olmasını sağlamaktı. 
FiniteStateMachine oluşturarak oyun içerisinde bulunan state'leri ileri implementasyonlar için ayırmaya çalıştım ve oyun içerisinde yaşanan eventleri takip ederek olabildiğince az Update() metodu kullanmaya çalıştım.
Manager'ları interface yardımıyla daha düzenli yapmaya çalıştım ve SOLID principle kurallarını takip ederek her Manager'ın kendi yapması gereken metotları tutmasını sağlamaya çalıştım.
Sahnelerdeki entityleri, sahneye geçince Init edip, optimizasyonu ön plana aldım ve PoolManager kullanarak, yolcuların ve otobüslerin yok olmaktansa, gizlenmesini sağlayarak ekstra bir optimizasyon öneclik almaya çalıştım.
Levelları düzenleyip, oluşturabilmek için Level Editor adında bir scene oluşturup, editor üzerinden kolayca seviyeler üretebilmek için LevelEditorController oluşturdum

**Third Party Assets**

Zemin Tile modeli ve ToonShader/OutlineShader harici bütün 3D modeller ve SFX'ler bedava olan assetler yardımıyla yapıldı.

**Utilities**

DOTween (HOTween v2)
Extenject Dependency Injection IOC

**Not:** Scriptlerin refactoringi, LevelEditorController'a görsel arayüzü sağlayabilme ve önceden belirlemiş olduğum Architecture üzerinde tavsiyeler almak için yapay zekanın yardımı alınmıştır

